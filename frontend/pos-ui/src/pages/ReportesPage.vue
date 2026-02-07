<template>
  <div class="reportes-page">
    <v-card v-if="!canView" class="pos-card pa-4">
      <div class="text-h6">Reportes</div>
      <div class="text-caption text-medium-emphasis">No tenes permisos para ver reportes.</div>
    </v-card>

    <template v-else>
      <v-card class="pos-card pa-4 mb-4">
        <div class="d-flex flex-wrap align-center gap-3">
          <div>
            <div class="text-h6">Dashboard de reportes</div>
            <div class="text-caption text-medium-emphasis">Rangos y KPIs</div>
          </div>
          <v-spacer />
          <v-text-field
            v-model="filters.desde"
            label="Desde"
            type="date"
            variant="outlined"
            density="comfortable"
            hide-details
            style="max-width: 180px"
          />
          <v-text-field
            v-model="filters.hasta"
            label="Hasta"
            type="date"
            variant="outlined"
            density="comfortable"
            hide-details
            style="max-width: 180px"
          />
          <v-text-field
            v-model.number="filters.dias"
            label="Dias inmovilizado"
            type="number"
            min="1"
            step="1"
            variant="outlined"
            density="comfortable"
            hide-details
            style="max-width: 160px"
          />
          <v-btn
            color="primary"
            class="text-none"
            :loading="loading"
            @click="loadReports"
          >
            Actualizar
          </v-btn>
        </div>
      </v-card>

      <v-row dense class="mb-4">
        <v-col cols="12" md="3">
          <v-card class="pos-card pa-4">
            <div class="text-caption text-medium-emphasis">Ventas</div>
            <div v-if="loading" class="mt-2">
              <v-skeleton-loader type="text" />
            </div>
            <div v-else class="text-h5">{{ formatMoney(kpis.totalVentas) }}</div>
          </v-card>
        </v-col>
        <v-col cols="12" md="3">
          <v-card class="pos-card pa-4">
            <div class="text-caption text-medium-emphasis">Medios de pago</div>
            <div v-if="loading" class="mt-2">
              <v-skeleton-loader type="text" />
            </div>
            <div v-else class="text-h5">{{ formatMoney(kpis.totalMedios) }}</div>
          </v-card>
        </v-col>
        <v-col cols="12" md="3">
          <v-card class="pos-card pa-4">
            <div class="text-caption text-medium-emphasis">Top producto</div>
            <div v-if="loading" class="mt-2">
              <v-skeleton-loader type="text" />
            </div>
            <div v-else class="text-h6">{{ kpis.topProducto }}</div>
          </v-card>
        </v-col>
        <v-col cols="12" md="3">
          <v-card class="pos-card pa-4">
            <div class="text-caption text-medium-emphasis">Rotacion neta</div>
            <div v-if="loading" class="mt-2">
              <v-skeleton-loader type="text" />
            </div>
            <div v-else class="text-h5">{{ formatNumber(kpis.rotacionNeta) }}</div>
          </v-card>
        </v-col>
      </v-row>

      <v-row dense class="mb-4">
        <v-col cols="12" md="6">
          <v-card class="pos-card pa-4">
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h6">Ventas por dia</div>
                <div class="text-caption text-medium-emphasis">Linea</div>
              </div>
            </div>
            <div v-if="loading" class="mt-3">
              <v-skeleton-loader type="image" height="180" />
            </div>
            <div v-else class="chart-wrapper mt-3">
              <svg viewBox="0 0 100 40" preserveAspectRatio="none" class="chart-svg">
                <polyline
                  :points="ventasLine"
                  fill="none"
                  stroke="#0f766e"
                  stroke-width="2"
                />
                <circle
                  v-for="(point, idx) in ventasPoints"
                  :key="idx"
                  :cx="point.x"
                  :cy="point.y"
                  r="1.8"
                  fill="#0ea5a4"
                />
              </svg>
              <div class="chart-labels">
                <span v-for="label in ventasLabels" :key="label">{{ label }}</span>
              </div>
            </div>
          </v-card>
        </v-col>
        <v-col cols="12" md="6">
          <v-card class="pos-card pa-4">
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h6">Medios de pago</div>
                <div class="text-caption text-medium-emphasis">Barras</div>
              </div>
            </div>
            <div v-if="loading" class="mt-3">
              <v-skeleton-loader type="image" height="180" />
            </div>
            <div v-else class="chart-wrapper mt-3">
              <svg viewBox="0 0 100 40" preserveAspectRatio="none" class="chart-svg">
                <rect
                  v-for="(bar, idx) in mediosBars"
                  :key="idx"
                  :x="bar.x"
                  :y="bar.y"
                  :width="bar.width"
                  :height="bar.height"
                  fill="#ea580c"
                  rx="1"
                />
              </svg>
              <div class="chart-labels">
                <span v-for="label in mediosLabels" :key="label">{{ label }}</span>
              </div>
            </div>
          </v-card>
        </v-col>
      </v-row>

      <v-row dense>
        <v-col cols="12" md="4">
          <v-card class="pos-card pa-4">
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h6">Top productos</div>
                <div class="text-caption text-medium-emphasis">Por ventas</div>
              </div>
              <v-btn variant="tonal" color="primary" class="text-none" @click="exportCsv('top-productos')">
                Exportar CSV
              </v-btn>
            </div>
            <div v-if="loading" class="mt-3">
              <v-skeleton-loader type="table" />
            </div>
            <v-data-table
              v-else
              class="mt-3"
              :headers="topHeaders"
              :items="topProductos"
              density="compact"
              item-key="productoId"
            />
          </v-card>
        </v-col>

        <v-col cols="12" md="4">
          <v-card class="pos-card pa-4">
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h6">Rotacion</div>
                <div class="text-caption text-medium-emphasis">Entradas vs salidas</div>
              </div>
              <v-btn variant="tonal" color="primary" class="text-none" @click="exportCsv('rotacion')">
                Exportar CSV
              </v-btn>
            </div>
            <div v-if="loading" class="mt-3">
              <v-skeleton-loader type="table" />
            </div>
            <v-data-table
              v-else
              class="mt-3"
              :headers="rotacionHeaders"
              :items="rotacion"
              density="compact"
              item-key="productoId"
            />
          </v-card>
        </v-col>

        <v-col cols="12" md="4">
          <v-card class="pos-card pa-4">
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h6">Inmovilizado</div>
                <div class="text-caption text-medium-emphasis">Sin movimientos</div>
              </div>
              <v-btn variant="tonal" color="primary" class="text-none" @click="exportCsv('inmovilizado')">
                Exportar CSV
              </v-btn>
            </div>
            <div v-if="loading" class="mt-3">
              <v-skeleton-loader type="table" />
            </div>
            <v-data-table
              v-else
              class="mt-3"
              :headers="inmovilizadoHeaders"
              :items="inmovilizado"
              density="compact"
              item-key="productoId"
            />
          </v-card>
        </v-col>
      </v-row>
    </template>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" location="top end" timeout="1800">
      <div class="d-flex align-center gap-2">
        <v-icon>{{ snackbar.icon }}</v-icon>
        <span>{{ snackbar.text }}</span>
      </div>
    </v-snackbar>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { getJson } from '../services/apiClient';

const auth = useAuthStore();
const canView = computed(() => auth.hasPermission('PERM_REPORTES_VER'));

const today = new Date();
const sevenDaysAgo = new Date();
sevenDaysAgo.setDate(today.getDate() - 7);

const filters = reactive({
  desde: sevenDaysAgo.toISOString().slice(0, 10),
  hasta: today.toISOString().slice(0, 10),
  dias: 30
});

const loading = ref(false);
const ventasChart = ref(null);
const mediosChart = ref(null);
const topProductos = ref([]);
const rotacion = ref([]);
const inmovilizado = ref([]);

const snackbar = ref({
  show: false,
  text: '',
  color: 'success',
  icon: 'mdi-check-circle'
});

const topHeaders = [
  { title: 'Producto', value: 'nombre' },
  { title: 'SKU', value: 'sku' },
  { title: 'Cantidad', value: 'cantidad', align: 'end' },
  { title: 'Total', value: 'total', align: 'end' }
];

const rotacionHeaders = [
  { title: 'Producto', value: 'nombre' },
  { title: 'SKU', value: 'sku' },
  { title: 'Entradas', value: 'entradas', align: 'end' },
  { title: 'Salidas', value: 'salidas', align: 'end' },
  { title: 'Neto', value: 'neto', align: 'end' }
];

const inmovilizadoHeaders = [
  { title: 'Producto', value: 'nombre' },
  { title: 'SKU', value: 'sku' },
  { title: 'Stock', value: 'stockActual', align: 'end' },
  { title: 'Ultimo mov', value: 'ultimoMovimiento' },
  { title: 'Dias', value: 'diasSinMovimiento', align: 'end' }
];

const formatMoney = (value) =>
  new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(value || 0);

const formatNumber = (value) =>
  new Intl.NumberFormat('es-AR', { maximumFractionDigits: 0 }).format(value || 0);

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

const ventasLabels = computed(() => ventasChart.value?.labels || []);
const ventasSeries = computed(() => ventasChart.value?.series?.[0]?.data || []);
const mediosLabels = computed(() => mediosChart.value?.labels || []);
const mediosSeries = computed(() => mediosChart.value?.series?.[0]?.data || []);

const buildLinePoints = (values) => {
  if (!values.length) return [];
  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;
  return values.map((value, index) => {
    const x = (index / Math.max(values.length - 1, 1)) * 100;
    const y = 35 - ((value - min) / range) * 25;
    return { x, y };
  });
};

const ventasPoints = computed(() => buildLinePoints(ventasSeries.value));
const ventasLine = computed(() => ventasPoints.value.map((point) => `${point.x},${point.y}`).join(' '));

const mediosBars = computed(() => {
  const values = mediosSeries.value;
  if (!values.length) return [];
  const max = Math.max(...values) || 1;
  const barWidth = 100 / values.length - 4;
  return values.map((value, index) => {
    const height = (value / max) * 28;
    return {
      x: index * (barWidth + 4) + 2,
      y: 35 - height,
      width: barWidth,
      height
    };
  });
});

const kpis = computed(() => {
  const totalVentas = ventasSeries.value.reduce((acc, value) => acc + value, 0);
  const totalMedios = mediosSeries.value.reduce((acc, value) => acc + value, 0);
  const topProducto = topProductos.value?.[0]?.nombre || '-';
  const rotacionNeta = rotacion.value.reduce((acc, item) => acc + (item.neto || 0), 0);

  return { totalVentas, totalMedios, topProducto, rotacionNeta };
});

const buildQuery = () => {
  const params = new URLSearchParams();
  if (filters.desde) params.set('desde', filters.desde);
  if (filters.hasta) params.set('hasta', filters.hasta);
  return params.toString();
};

const loadReports = async () => {
  if (!canView.value) return;
  loading.value = true;
  try {
    const query = buildQuery();

    const dias = Number(filters.dias || 30);
    const [ventasResp, mediosResp, topResp, rotResp, inmResp] = await Promise.all([
      getJson(`/api/v1/reportes/ventas-por-dia?${query}`),
      getJson(`/api/v1/reportes/medios-pago?${query}`),
      getJson(`/api/v1/reportes/top-productos?${query}&top=10`),
      getJson(`/api/v1/reportes/rotacion-stock?${query}`),
      getJson(`/api/v1/reportes/stock-inmovilizado?dias=${dias}`)
    ]);

    if (!ventasResp.response.ok) throw new Error(extractProblemMessage(ventasResp.data));
    if (!mediosResp.response.ok) throw new Error(extractProblemMessage(mediosResp.data));
    if (!topResp.response.ok) throw new Error(extractProblemMessage(topResp.data));
    if (!rotResp.response.ok) throw new Error(extractProblemMessage(rotResp.data));
    if (!inmResp.response.ok) throw new Error(extractProblemMessage(inmResp.data));

    ventasChart.value = ventasResp.data;
    mediosChart.value = mediosResp.data;
    topProductos.value = topResp.data?.rows || [];
    rotacion.value = rotResp.data?.rows || [];
    inmovilizado.value = (inmResp.data?.rows || []).map((row) => ({
      ...row,
      ultimoMovimiento: row.ultimoMovimiento ? new Date(row.ultimoMovimiento).toLocaleDateString('es-AR') : '-'
    }));
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar los reportes.');
  } finally {
    loading.value = false;
  }
};

const toCsv = (rows, headers) => {
  const headerLine = headers.map((header) => `"${header.title}"`).join(',');
  const lines = rows.map((row) =>
    headers
      .map((header) => {
        const value = row[header.value] ?? '';
        return `"${String(value).replace(/"/g, '""')}"`;
      })
      .join(',')
  );

  return [headerLine, ...lines].join('\n');
};

const downloadFile = (content, filename) => {
  const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(url);
};

const exportCsv = (type) => {
  if (type === 'top-productos') {
    downloadFile(toCsv(topProductos.value, topHeaders), 'top-productos.csv');
  }
  if (type === 'rotacion') {
    downloadFile(toCsv(rotacion.value, rotacionHeaders), 'rotacion-stock.csv');
  }
  if (type === 'inmovilizado') {
    downloadFile(toCsv(inmovilizado.value, inmovilizadoHeaders), 'stock-inmovilizado.csv');
  }
};

onMounted(() => {
  loadReports();
});
</script>

<style scoped>
.reportes-page {
  animation: fade-in 0.3s ease;
}

.chart-wrapper {
  border: 1px solid rgba(15, 23, 42, 0.08);
  border-radius: 12px;
  padding: 12px;
  background: #fff;
}

.chart-svg {
  width: 100%;
  height: 180px;
}

.chart-labels {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  color: #475569;
  margin-top: 6px;
}
</style>
