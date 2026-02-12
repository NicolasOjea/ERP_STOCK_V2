<template>
  <div class="remitos-page">
    <v-card class="pos-card pa-4">
      <div class="d-flex align-center gap-3">
        <div>
          <div class="text-h6">Cargar remito</div>
          <div class="text-caption text-medium-emphasis">Pegue JSON o suba archivo</div>
        </div>
      </div>

      <v-alert
        v-if="error"
        type="error"
        variant="tonal"
        class="mt-3"
        density="compact"
      >
        {{ error }}
      </v-alert>

      <v-textarea
        v-model="jsonInput"
        label="JSON remito"
        variant="outlined"
        density="comfortable"
        rows="10"
        class="mt-4"
      />

      <v-file-input
        v-model="fileInput"
        label="Archivo (json)"
        variant="outlined"
        density="comfortable"
        class="mt-3"
        prepend-icon="mdi-paperclip"
        @update:model-value="handleFile"
      />

      <div class="d-flex align-center gap-3 mt-3">
        <v-btn
          color="primary"
          size="large"
          class="text-none"
          :loading="parseLoading"
          @click="parsear"
        >
          Parsear
        </v-btn>
        <v-btn
          variant="tonal"
          color="secondary"
          class="text-none"
          @click="setSample"
        >
          Usar sample
        </v-btn>
      </div>

      <v-divider class="my-4" />

      <div v-if="parseResult" class="text-caption text-medium-emphasis">
        Documento: {{ shortId(parseResult.documentoCompraId) }} - Items: {{ parseResult.items?.length || 0 }}
      </div>
    </v-card>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" location="top end" timeout="1700">
      <div class="d-flex align-center gap-2">
        <v-icon>{{ snackbar.icon }}</v-icon>
        <span>{{ snackbar.text }}</span>
      </div>
    </v-snackbar>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { postJson } from '../services/apiClient';
import { useRouter } from 'vue-router';

const router = useRouter();

const jsonInput = ref('');
const fileInput = ref(null);
const parseLoading = ref(false);
const parseResult = ref(null);
const error = ref('');

const snackbar = ref({
  show: false,
  text: '',
  color: 'success',
  icon: 'mdi-check-circle'
});

const shortId = (value) => {
  if (!value) return 'n/a';
  return value.length > 8 ? value.slice(0, 8) : value;
};

const flash = (type, text) => {
  snackbar.value = {
    show: true,
    text,
    color: type === 'success' ? 'success' : 'error',
    icon: type === 'success' ? 'mdi-check-circle' : 'mdi-alert-circle'
  };
};

const extractProblemMessage = (data) => {
  if (!data) return 'Error inesperado.';
  if (data.errors) {
    const firstKey = Object.keys(data.errors)[0];
    if (firstKey && data.errors[firstKey]?.length) {
      return `${firstKey}: ${data.errors[firstKey][0]}`;
    }
  }
  return data.detail || data.title || 'Error inesperado.';
};

const handleFile = async (files) => {
  error.value = '';
  const file = Array.isArray(files) ? files[0] : files;
  if (!file) return;
  try {
    const text = await file.text();
    jsonInput.value = text;
  } catch {
    error.value = 'No se pudo leer el archivo.';
  }
};

const setSample = () => {
  jsonInput.value = JSON.stringify(
    {
      proveedorId: null,
      numero: 'REM-0001',
      fecha: new Date().toISOString().slice(0, 10),
      items: [
        { sku: '779123456001', descripcion: 'Yerba 1kg', cantidad: 2, costoUnitario: 1200 },
        { sku: '779123456999', descripcion: 'Producto desconocido', cantidad: 1 }
      ]
    },
    null,
    2
  );
};

const parsear = async () => {
  if (parseLoading.value) return;
  error.value = '';
  if (!jsonInput.value.trim()) {
    error.value = 'Debe pegar un JSON valido.';
    return;
  }

  let payload;
  try {
    payload = JSON.parse(jsonInput.value);
  } catch (err) {
    error.value = 'JSON invalido.';
    return;
  }

  parseLoading.value = true;
  try {
    const { response, data } = await postJson('/api/v1/documentos-compra/parse', payload);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    parseResult.value = data;

    const preResp = await postJson('/api/v1/pre-recepciones', {
      documentoCompraId: data.documentoCompraId
    });

    if (!preResp.response.ok) {
      throw new Error(extractProblemMessage(preResp.data));
    }

    flash('success', 'Remito parseado');
    router.push(`/remitos/pre-recepcion/${preResp.data.id}`);
  } catch (err) {
    flash('error', err?.message || 'No se pudo parsear el remito.');
  } finally {
    parseLoading.value = false;
  }
};
</script>

<style scoped>
.remitos-page {
  animation: fade-in 0.3s ease;
}
</style>
