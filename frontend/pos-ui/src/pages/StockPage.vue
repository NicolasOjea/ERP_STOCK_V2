<template>
  <div class="stock-page">
    <v-card class="pos-card pa-4 mb-4">
      <div class="d-flex align-center gap-3">
        <div>
          <div class="text-h6">Stock</div>
          <div class="text-caption text-medium-emphasis">Saldos, movimientos y alertas</div>
        </div>
      </div>
    </v-card>

    <v-tabs v-model="tab" color="primary" class="mb-3">
      <v-tab value="saldos">Saldos</v-tab>
      <v-tab value="movimientos">Movimientos</v-tab>
      <v-tab value="alertas">Alertas</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item value="saldos">
        <v-card class="pos-card pa-4">
          <div class="d-flex align-center gap-3">
            <v-text-field
              v-model="saldoSearch"
              label="Buscar producto"
              variant="outlined"
              density="comfortable"
              hide-details
              style="max-width: 280px"
            />
            <v-btn
              color="primary"
              variant="tonal"
              class="text-none"
              :loading="saldosLoading"
              @click="loadSaldos"
            >
              Buscar
            </v-btn>
          </div>

          <v-data-table
            class="mt-3"
            :headers="saldoHeaders"
            :items="saldos"
            item-key="productoId"
            density="compact"
            height="520"
          />
        </v-card>
      </v-window-item>

      <v-window-item value="movimientos">
        <v-card class="pos-card pa-4">
          <div class="d-flex flex-wrap align-center gap-3">
            <v-text-field
              v-model="movFilters.productoId"
              label="Producto Id"
              variant="outlined"
              density="comfortable"
              hide-details
              style="min-width: 240px"
            />
            <v-text-field
              v-model="movFilters.desde"
              label="Desde"
              type="date"
              variant="outlined"
              density="comfortable"
              hide-details
            />
            <v-text-field
              v-model="movFilters.hasta"
              label="Hasta"
              type="date"
              variant="outlined"
              density="comfortable"
              hide-details
            />
            <v-btn
              color="primary"
              variant="tonal"
              class="text-none"
              :loading="movLoading"
              @click="loadMovimientos"
            >
              Buscar
            </v-btn>
          </div>

          <v-expansion-panels class="mt-3" variant="accordion">
            <v-expansion-panel v-for="mov in movimientos" :key="mov.id">
              <v-expansion-panel-title>
                <div class="d-flex align-center justify-space-between w-100">
                  <div>
                    <div class="text-subtitle-2">{{ mov.tipo }}</div>
                    <div class="text-caption text-medium-emphasis">{{ mov.motivo }}</div>
                  </div>
                  <div class="text-caption text-medium-emphasis">{{ formatDate(mov.fecha) }}</div>
                </div>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-list density="compact">
                  <v-list-item v-for="item in mov.items" :key="item.id">
                    <v-list-item-title>{{ item.nombre }} ({{ item.sku }})</v-list-item-title>
                    <v-list-item-subtitle>
                      Cantidad: {{ item.cantidad }} - {{ item.esIngreso ? 'Ingreso' : 'Egreso' }}
                    </v-list-item-subtitle>
                  </v-list-item>
                </v-list>
              </v-expansion-panel-text>
            </v-expansion-panel>
          </v-expansion-panels>

          <div v-if="!movimientos.length" class="text-caption text-medium-emphasis mt-3">
            Sin movimientos.
          </div>
        </v-card>
      </v-window-item>

      <v-window-item value="alertas">
        <v-card class="pos-card pa-4">
          <div class="d-flex align-center gap-3">
            <v-btn
              color="primary"
              variant="tonal"
              class="text-none"
              :loading="alertasLoading"
              @click="loadAlertas"
            >
              Actualizar
            </v-btn>
          </div>

          <v-list density="compact" class="mt-3">
            <v-list-item v-for="alerta in alertas" :key="alerta.productoId">
              <v-list-item-title>{{ alerta.nombre }}</v-list-item-title>
              <v-list-item-subtitle>
                Stock: {{ alerta.cantidadActual }} / Min: {{ alerta.stockMinimo }}
              </v-list-item-subtitle>
              <template #append>
                <v-chip
                  size="small"
                  class="status-chip"
                  :color="alerta.nivel === 'CRITICO' ? 'error' : 'warning'"
                  variant="tonal"
                >
                  {{ alerta.nivel }}
                </v-chip>
              </template>
            </v-list-item>
          </v-list>

          <div v-if="!alertas.length" class="text-caption text-medium-emphasis mt-3">
            Sin alertas.
          </div>
        </v-card>
      </v-window-item>
    </v-window>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" location="top end" timeout="1700">
      <div class="d-flex align-center gap-2">
        <v-icon>{{ snackbar.icon }}</v-icon>
        <span>{{ snackbar.text }}</span>
      </div>
    </v-snackbar>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, watch } from 'vue';
import { getJson } from '../services/apiClient';

const tab = ref('saldos');

const saldos = ref([]);
const saldosLoading = ref(false);
const saldoSearch = ref('');

const movimientos = ref([]);
const movLoading = ref(false);
const movFilters = reactive({
  productoId: '',
  desde: '',
  hasta: ''
});

const alertas = ref([]);
const alertasLoading = ref(false);

const snackbar = ref({
  show: false,
  text: '',
  color: 'success',
  icon: 'mdi-check-circle'
});

const saldoHeaders = [
  { title: 'Producto', value: 'nombre' },
  { title: 'SKU', value: 'sku' },
  { title: 'Cantidad', value: 'cantidadActual', align: 'end' }
];

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

const formatDate = (value) => {
  if (!value) return '-';
  try {
    return new Date(value).toLocaleString('es-AR');
  } catch {
    return value;
  }
};

const loadSaldos = async () => {
  saldosLoading.value = true;
  try {
    const params = new URLSearchParams();
    if (saldoSearch.value.trim()) params.set('search', saldoSearch.value.trim());
    const { response, data } = await getJson(`/api/v1/stock/saldos?${params.toString()}`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    saldos.value = data || [];
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar saldos.');
  } finally {
    saldosLoading.value = false;
  }
};

const loadMovimientos = async () => {
  movLoading.value = true;
  try {
    const params = new URLSearchParams();
    if (movFilters.productoId.trim()) params.set('productoId', movFilters.productoId.trim());
    if (movFilters.desde) params.set('desde', movFilters.desde);
    if (movFilters.hasta) params.set('hasta', movFilters.hasta);

    const { response, data } = await getJson(`/api/v1/stock/movimientos?${params.toString()}`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    movimientos.value = data || [];
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar movimientos.');
  } finally {
    movLoading.value = false;
  }
};

const loadAlertas = async () => {
  alertasLoading.value = true;
  try {
    const { response, data } = await getJson('/api/v1/stock/alertas');
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    alertas.value = data || [];
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar alertas.');
  } finally {
    alertasLoading.value = false;
  }
};

watch(tab, (value) => {
  if (value === 'saldos') {
    loadSaldos();
  }
  if (value === 'movimientos') {
    loadMovimientos();
  }
  if (value === 'alertas') {
    loadAlertas();
  }
});

onMounted(() => {
  loadSaldos();
});
</script>

<style scoped>
.stock-page {
  animation: fade-in 0.3s ease;
}
</style>
