<template>
  <div class="productos-page">
    <v-card class="pos-card pa-4 mb-4">
      <div class="d-flex flex-wrap align-center gap-3">
        <div>
          <div class="text-h6">Productos</div>
          <div class="text-caption text-medium-emphasis">ABM y codigos</div>
        </div>
        <v-spacer />
        <v-btn color="primary" class="text-none" @click="resetForm">Nuevo</v-btn>
      </div>

      <div class="mt-4 d-flex flex-wrap align-center gap-3">
        <v-text-field
          v-model="search"
          label="Buscar (nombre, SKU, codigo)"
          variant="outlined"
          density="comfortable"
          hide-details
          style="min-width: 260px"
        />
        <v-text-field
          v-model="categoriaId"
          label="Categoria Id"
          variant="outlined"
          density="comfortable"
          hide-details
          style="min-width: 220px"
        />
        <v-btn-toggle
          v-model="activoFilter"
          density="comfortable"
          mandatory
          class="ml-2"
        >
          <v-btn value="all" class="text-none">Todos</v-btn>
          <v-btn value="true" class="text-none">Activos</v-btn>
          <v-btn value="false" class="text-none">Inactivos</v-btn>
        </v-btn-toggle>
        <v-btn
          color="primary"
          variant="tonal"
          class="text-none"
          :loading="loading"
          @click="loadProducts"
        >
          Buscar
        </v-btn>
      </div>
    </v-card>

    <v-row dense>
      <v-col cols="12" md="7">
        <v-card class="pos-card pa-4">
          <div class="text-h6">Listado</div>
          <div class="text-caption text-medium-emphasis">Selecciona un producto para editar</div>
          <v-data-table
            :headers="headers"
            :items="products"
            item-key="id"
            class="mt-3"
            density="compact"
            height="520"
            @click:row="selectProduct"
          >
            <template #item.isActive="{ item }">
              <v-chip
                size="small"
                :color="item.isActive ? 'success' : 'error'"
                variant="tonal"
              >
                {{ item.isActive ? 'Activo' : 'Inactivo' }}
              </v-chip>
            </template>
          </v-data-table>
        </v-card>
      </v-col>

      <v-col cols="12" md="5">
        <v-card class="pos-card pa-4">
          <div class="d-flex align-center justify-space-between">
            <div>
              <div class="text-h6">{{ form.id ? 'Editar producto' : 'Nuevo producto' }}</div>
              <div class="text-caption text-medium-emphasis">
                {{ form.id ? shortId(form.id) : 'Sin seleccionar' }}
              </div>
            </div>
            <v-btn
              color="primary"
              variant="tonal"
              class="text-none"
              :loading="saving"
              @click="saveProduct"
            >
              Guardar
            </v-btn>
          </div>

          <v-form class="mt-3">
            <v-text-field
              v-model="form.name"
              label="Nombre"
              variant="outlined"
              density="comfortable"
              :error-messages="errors.name"
              @blur="validateField('name')"
              required
            />
            <v-text-field
              v-model="form.sku"
              label="SKU"
              variant="outlined"
              density="comfortable"
              :error-messages="errors.sku"
              @blur="validateField('sku')"
              required
            />
            <v-text-field
              v-model="form.categoriaId"
              label="Categoria Id"
              variant="outlined"
              density="comfortable"
              :error-messages="errors.categoriaId"
              @blur="validateField('categoriaId')"
            />
            <v-text-field
              v-model="form.marcaId"
              label="Marca Id"
              variant="outlined"
              density="comfortable"
              :error-messages="errors.marcaId"
              @blur="validateField('marcaId')"
            />
            <v-text-field
              v-model="form.proveedorId"
              label="Proveedor Id"
              variant="outlined"
              density="comfortable"
              :error-messages="errors.proveedorId"
              @blur="validateField('proveedorId')"
            />
            <v-text-field
              v-model="form.precioBase"
              label="Precio base"
              variant="outlined"
              density="comfortable"
              :error-messages="errors.precioBase"
              @blur="validateField('precioBase')"
            />
            <v-switch
              :model-value="form.isActive"
              label="Activo"
              color="primary"
              inset
              @update:model-value="toggleActive"
            />
          </v-form>

          <v-divider class="my-4" />

          <div class="text-subtitle-2">Codigos</div>
          <div class="text-caption text-medium-emphasis">Gestion de codigos asociados</div>
          <div class="d-flex align-center gap-2 mt-2">
            <v-text-field
              v-model="newCode"
              label="Nuevo codigo"
              variant="outlined"
              density="comfortable"
              hide-details
              style="flex: 1"
            />
            <v-btn
              color="secondary"
              class="text-none"
              :disabled="!form.id"
              :loading="codeLoading"
              @click="addCode"
            >
              Agregar
            </v-btn>
          </div>
          <v-list density="compact" class="mt-2">
            <v-list-item v-for="code in codes" :key="code.id">
              <v-list-item-title>{{ code.code }}</v-list-item-title>
              <template #append>
                <v-btn
                  icon="mdi-delete"
                  variant="text"
                  color="error"
                  :disabled="!form.id"
                  @click="removeCode(code)"
                />
              </template>
            </v-list-item>
          </v-list>

          <v-divider class="my-4" />

          <div class="text-subtitle-2">Config stock</div>
          <div class="text-caption text-medium-emphasis">Por sucursal actual</div>
          <v-text-field
            v-model="stockConfig.stockMinimo"
            label="Stock minimo"
            type="number"
            min="0"
            step="0.01"
            variant="outlined"
            density="comfortable"
            :error-messages="errors.stockMinimo"
            @blur="validateField('stockMinimo')"
          />
          <v-text-field
            v-model="stockConfig.toleranciaPct"
            label="Tolerancia"
            type="number"
            min="0"
            step="0.01"
            variant="outlined"
            density="comfortable"
            :error-messages="errors.toleranciaPct"
            @blur="validateField('toleranciaPct')"
          />
          <v-btn
            color="primary"
            variant="tonal"
            class="text-none"
            :disabled="!form.id"
            :loading="stockLoading"
            @click="updateStockConfig"
          >
            Guardar config
          </v-btn>
        </v-card>
      </v-col>
    </v-row>

    <v-dialog v-model="dialogDesactivar" width="420">
      <v-card>
        <v-card-title>Desactivar producto</v-card-title>
        <v-card-text>
          Esta accion desactiva el producto y lo oculta de operaciones. Continuar?
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn variant="text" @click="cancelDeactivate">Cancelar</v-btn>
          <v-btn color="error" @click="confirmDeactivate">Desactivar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" location="top end" timeout="1700">
      <div class="d-flex align-center gap-2">
        <v-icon>{{ snackbar.icon }}</v-icon>
        <span>{{ snackbar.text }}</span>
      </div>
    </v-snackbar>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { getJson, postJson, requestJson } from '../services/apiClient';

const products = ref([]);
const loading = ref(false);
const saving = ref(false);
const codeLoading = ref(false);
const stockLoading = ref(false);

const search = ref('');
const categoriaId = ref('');
const activoFilter = ref('all');

const form = reactive({
  id: '',
  name: '',
  sku: '',
  categoriaId: '',
  marcaId: '',
  proveedorId: '',
  isActive: true,
  precioBase: ''
});

const stockConfig = reactive({
  stockMinimo: '',
  toleranciaPct: ''
});

const errors = reactive({
  name: '',
  sku: '',
  categoriaId: '',
  marcaId: '',
  proveedorId: '',
  precioBase: '',
  stockMinimo: '',
  toleranciaPct: ''
});

const codes = ref([]);
const newCode = ref('');

const dialogDesactivar = ref(false);
const pendingActive = ref(true);

const snackbar = ref({
  show: false,
  text: '',
  color: 'success',
  icon: 'mdi-check-circle'
});

const headers = [
  { title: 'Nombre', value: 'name' },
  { title: 'SKU', value: 'sku' },
  { title: 'Categoria', value: 'categoria' },
  { title: 'Marca', value: 'marca' },
  { title: 'Proveedor', value: 'proveedor' },
  { title: 'Estado', value: 'isActive' }
];

const isGuid = (value) =>
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(value);

const parseGuidOrNull = (value) => {
  const trimmed = (value || '').trim();
  if (!trimmed) return null;
  return isGuid(trimmed) ? trimmed : 'INVALID';
};

const formatMoney = (value) =>
  new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(value || 0);

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

const clearErrors = () => {
  Object.keys(errors).forEach((key) => {
    errors[key] = '';
  });
};

const validateField = (field) => {
  if (field === 'name') {
    errors.name = form.name.trim() ? '' : 'El nombre es obligatorio.';
  }
  if (field === 'sku') {
    errors.sku = form.sku.trim() ? '' : 'El SKU es obligatorio.';
  }
  if (field === 'categoriaId') {
    const value = parseGuidOrNull(form.categoriaId);
    errors.categoriaId = value === 'INVALID' ? 'Categoria Id invalido.' : '';
  }
  if (field === 'marcaId') {
    const value = parseGuidOrNull(form.marcaId);
    errors.marcaId = value === 'INVALID' ? 'Marca Id invalido.' : '';
  }
  if (field === 'proveedorId') {
    const value = parseGuidOrNull(form.proveedorId);
    errors.proveedorId = value === 'INVALID' ? 'Proveedor Id invalido.' : '';
  }
  if (field === 'precioBase') {
    if (form.precioBase === '') {
      errors.precioBase = '';
    } else if (Number(form.precioBase) < 0 || Number.isNaN(Number(form.precioBase))) {
      errors.precioBase = 'Precio base invalido.';
    } else {
      errors.precioBase = '';
    }
  }
  if (field === 'stockMinimo') {
    if (stockConfig.stockMinimo === '') {
      errors.stockMinimo = '';
    } else if (Number(stockConfig.stockMinimo) < 0 || Number.isNaN(Number(stockConfig.stockMinimo))) {
      errors.stockMinimo = 'Stock minimo invalido.';
    } else {
      errors.stockMinimo = '';
    }
  }
  if (field === 'toleranciaPct') {
    if (stockConfig.toleranciaPct === '') {
      errors.toleranciaPct = '';
    } else if (Number(stockConfig.toleranciaPct) <= 0 || Number.isNaN(Number(stockConfig.toleranciaPct))) {
      errors.toleranciaPct = 'Tolerancia invalida.';
    } else {
      errors.toleranciaPct = '';
    }
  }
};

const validateForm = () => {
  validateField('name');
  validateField('sku');
  validateField('categoriaId');
  validateField('marcaId');
  validateField('proveedorId');
  validateField('precioBase');
  return !errors.name && !errors.sku && !errors.categoriaId && !errors.marcaId && !errors.proveedorId && !errors.precioBase;
};

const loadProducts = async () => {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    if (search.value.trim()) params.set('search', search.value.trim());
    if (categoriaId.value.trim()) params.set('categoriaId', categoriaId.value.trim());
    if (activoFilter.value !== 'all') params.set('activo', activoFilter.value);

    const { response, data } = await getJson(`/api/v1/productos?${params.toString()}`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    products.value = data || [];
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar los productos.');
  } finally {
    loading.value = false;
  }
};

const selectProduct = async (event, row) => {
  const product = row?.item?.raw ?? row?.item ?? row;
  if (!product?.id) return;
  try {
    const { response, data } = await getJson(`/api/v1/productos/${product.id}`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    form.id = data.id;
    form.name = data.name || '';
    form.sku = data.sku || '';
    form.categoriaId = data.categoriaId || '';
    form.marcaId = data.marcaId || '';
    form.proveedorId = data.proveedorId || '';
    form.isActive = data.isActive ?? true;
    form.precioBase = '';
    codes.value = data.codes || [];
    stockConfig.stockMinimo = '';
    stockConfig.toleranciaPct = '';
    clearErrors();
  } catch (err) {
    flash('error', err?.message || 'No se pudo cargar el producto.');
  }
};

const resetForm = () => {
  form.id = '';
  form.name = '';
  form.sku = '';
  form.categoriaId = '';
  form.marcaId = '';
  form.proveedorId = '';
  form.isActive = true;
  form.precioBase = '';
  codes.value = [];
  newCode.value = '';
  stockConfig.stockMinimo = '';
  stockConfig.toleranciaPct = '';
  clearErrors();
};

const saveProduct = async () => {
  if (saving.value) return;
  if (!validateForm()) return;

  saving.value = true;
  try {
    const categoria = parseGuidOrNull(form.categoriaId);
    const marca = parseGuidOrNull(form.marcaId);
    const proveedor = parseGuidOrNull(form.proveedorId);

    if (categoria === 'INVALID' || marca === 'INVALID' || proveedor === 'INVALID') {
      flash('error', 'Hay campos con GUID invalido.');
      saving.value = false;
      return;
    }

    const precioBase = form.precioBase === '' ? null : Number(form.precioBase);

    if (!form.id) {
      const payload = {
        name: form.name.trim(),
        sku: form.sku.trim(),
        categoriaId: categoria,
        marcaId: marca,
        proveedorId: proveedor,
        isActive: form.isActive,
        precioBase
      };

      const { response, data } = await postJson('/api/v1/productos', payload);
      if (!response.ok) {
        throw new Error(extractProblemMessage(data));
      }
      form.id = data.id;
      codes.value = data.codes || [];
      flash('success', 'Producto creado');
      await loadProducts();
      return;
    }

    const payload = {
      name: form.name.trim() || null,
      sku: form.sku.trim() || null,
      categoriaId: categoria,
      marcaId: marca,
      proveedorId: proveedor,
      isActive: form.isActive,
      precioBase
    };

    const { response, data } = await requestJson(`/api/v1/productos/${form.id}`, {
      method: 'PATCH',
      body: JSON.stringify(payload)
    });
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    codes.value = data.codes || [];
    flash('success', 'Producto actualizado');
    await loadProducts();
  } catch (err) {
    flash('error', err?.message || 'No se pudo guardar el producto.');
  } finally {
    saving.value = false;
  }
};

const addCode = async () => {
  if (!form.id || !newCode.value.trim()) return;
  codeLoading.value = true;
  try {
    const payload = { code: newCode.value.trim() };
    const { response, data } = await postJson(`/api/v1/productos/${form.id}/codigos`, payload);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    codes.value = [...codes.value, data];
    newCode.value = '';
    flash('success', 'Codigo agregado');
  } catch (err) {
    flash('error', err?.message || 'No se pudo agregar el codigo.');
  } finally {
    codeLoading.value = false;
  }
};

const removeCode = async (code) => {
  if (!form.id) return;
  codeLoading.value = true;
  try {
    const { response, data } = await requestJson(`/api/v1/productos/${form.id}/codigos/${code.id}`, {
      method: 'DELETE'
    });
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    codes.value = codes.value.filter((item) => item.id !== code.id);
    flash('success', 'Codigo eliminado');
  } catch (err) {
    flash('error', err?.message || 'No se pudo eliminar el codigo.');
  } finally {
    codeLoading.value = false;
  }
};

const updateStockConfig = async () => {
  if (!form.id) return;
  validateField('stockMinimo');
  validateField('toleranciaPct');
  if (errors.stockMinimo || errors.toleranciaPct) return;
  if (stockConfig.stockMinimo === '' && stockConfig.toleranciaPct === '') {
    errors.stockMinimo = 'Debe indicar stock minimo o tolerancia.';
    return;
  }

  stockLoading.value = true;
  try {
    const payload = {
      stockMinimo: stockConfig.stockMinimo === '' ? null : Number(stockConfig.stockMinimo),
      toleranciaPct: stockConfig.toleranciaPct === '' ? null : Number(stockConfig.toleranciaPct)
    };

    const { response, data } = await requestJson(`/api/v1/productos/${form.id}/stock-config`, {
      method: 'PATCH',
      body: JSON.stringify(payload)
    });
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    stockConfig.stockMinimo = data.stockMinimo;
    stockConfig.toleranciaPct = data.toleranciaPct;
    flash('success', 'Config actualizada');
  } catch (err) {
    flash('error', err?.message || 'No se pudo actualizar el stock.');
  } finally {
    stockLoading.value = false;
  }
};

const toggleActive = (value) => {
  if (form.isActive && !value) {
    pendingActive.value = false;
    dialogDesactivar.value = true;
    return;
  }
  form.isActive = value;
};

const cancelDeactivate = () => {
  dialogDesactivar.value = false;
  form.isActive = true;
};

const confirmDeactivate = () => {
  dialogDesactivar.value = false;
  form.isActive = pendingActive.value;
};

onMounted(() => {
  loadProducts();
});
</script>

<style scoped>
.productos-page {
  animation: fade-in 0.3s ease;
}
</style>
