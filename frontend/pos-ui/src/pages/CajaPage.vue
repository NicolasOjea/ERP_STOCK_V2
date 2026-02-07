<template>
  <div class="caja-page">
    <v-card class="pos-card pa-4 mb-4">
      <div class="d-flex flex-wrap align-center gap-3">
        <div>
          <div class="text-h6">Caja</div>
          <div class="text-caption text-medium-emphasis">Gestion de sesiones</div>
        </div>
        <v-chip
          class="status-chip"
          :color="isAbierta ? 'success' : 'error'"
          variant="tonal"
        >
          {{ isAbierta ? 'ABIERTA' : 'CERRADA' }}
        </v-chip>
        <v-spacer />
        <div class="d-flex align-center gap-2 text-caption text-medium-emphasis">
          <v-icon size="16">mdi-storefront</v-icon>
          <span>Sucursal: {{ shortId(auth.sucursalId) }}</span>
        </div>
      </div>
    </v-card>

    <v-row dense>
      <v-col cols="12" md="6">
        <v-card class="pos-card pa-4 mb-4">
          <div class="text-h6">Apertura</div>
          <div class="text-caption text-medium-emphasis">Iniciar sesion de caja</div>
          <v-divider class="my-3" />

          <v-form @submit.prevent="abrirCaja">
            <v-text-field
              v-model="cajaId"
              label="Caja Id"
              variant="outlined"
              density="comfortable"
              :disabled="isAbierta"
              required
            />
            <v-text-field
              v-model.number="montoInicial"
              label="Monto inicial"
              type="number"
              min="0"
              step="0.01"
              variant="outlined"
              density="comfortable"
              :disabled="isAbierta"
              required
            />
            <v-btn
              color="primary"
              size="large"
              class="text-none"
              type="submit"
              :loading="openLoading"
              :disabled="isAbierta"
            >
              Abrir caja
            </v-btn>
          </v-form>

          <div v-if="session" class="mt-4">
            <div class="text-subtitle-2">Sesion actual</div>
            <div class="text-caption text-medium-emphasis">
              Id: {{ shortId(session.id) }}
            </div>
            <div class="text-caption text-medium-emphasis">
              Apertura: {{ formatDate(session.aperturaAt) }}
            </div>
          </div>
        </v-card>

        <v-card class="pos-card pa-4">
          <div class="text-h6">Movimientos</div>
          <div class="text-caption text-medium-emphasis">Retiro, gasto o ajuste</div>
          <v-divider class="my-3" />

          <v-form @submit.prevent="registrarMovimiento">
            <v-select
              v-model="movimiento.tipo"
              :items="tiposMovimiento"
              label="Tipo"
              variant="outlined"
              density="comfortable"
              :disabled="!isAbierta"
              required
            />
            <v-select
              v-model="movimiento.medioPago"
              :items="mediosPago"
              label="Medio"
              variant="outlined"
              density="comfortable"
              :disabled="!isAbierta"
            />
            <v-text-field
              v-model="movimiento.motivo"
              label="Motivo"
              variant="outlined"
              density="comfortable"
              :disabled="!isAbierta"
              required
            />
            <v-text-field
              v-model.number="movimiento.monto"
              label="Monto"
              type="number"
              step="0.01"
              variant="outlined"
              density="comfortable"
              :disabled="!isAbierta"
              required
            />
            <v-btn
              color="secondary"
              size="large"
              class="text-none"
              type="submit"
              :loading="movLoading"
              :disabled="!isAbierta"
            >
              Registrar movimiento
            </v-btn>
          </v-form>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card class="pos-card pa-4 mb-4">
          <div class="text-h6">Resumen en vivo</div>
          <div class="text-caption text-medium-emphasis">Totales de la sesion</div>
          <v-divider class="my-3" />

          <div v-if="resumen">
            <div class="d-flex justify-space-between">
              <span>Monto inicial</span>
              <strong>{{ formatMoney(resumen.montoInicial) }}</strong>
            </div>
            <div class="d-flex justify-space-between">
              <span>Ingresos</span>
              <strong>{{ formatMoney(resumen.totalIngresos) }}</strong>
            </div>
            <div class="d-flex justify-space-between">
              <span>Egresos</span>
              <strong>{{ formatMoney(resumen.totalEgresos) }}</strong>
            </div>
            <div class="d-flex justify-space-between text-h6 mt-2">
              <span>Saldo actual</span>
              <strong>{{ formatMoney(resumen.saldoActual) }}</strong>
            </div>
            <div class="text-caption text-medium-emphasis">
              Movimientos: {{ resumen.totalMovimientos }}
            </div>
          </div>
          <div v-else class="text-caption text-medium-emphasis">
            Sin resumen disponible.
          </div>

          <v-divider class="my-3" />
          <div class="text-subtitle-2">Por medio de pago</div>
          <v-list density="compact">
            <v-list-item v-for="medio in resumenMedios" :key="medio.medio">
              <v-list-item-title>{{ medio.medio }}</v-list-item-title>
              <v-list-item-subtitle>Teorico: {{ formatMoney(medio.teorico) }}</v-list-item-subtitle>
              <template #append>
                <strong>{{ formatMoney(medio.total) }}</strong>
              </template>
            </v-list-item>
          </v-list>

          <v-btn
            variant="tonal"
            color="primary"
            class="text-none mt-2"
            :loading="resumenLoading"
            :disabled="!session"
            @click="cargarResumen"
          >
            Actualizar resumen
          </v-btn>
        </v-card>

        <v-card class="pos-card pa-4">
          <div class="text-h6">Cierre</div>
          <div class="text-caption text-medium-emphasis">Arqueo y cierre de sesion</div>
          <v-divider class="my-3" />

          <div v-if="!isAbierta" class="text-caption text-medium-emphasis">
            No hay sesion abierta.
          </div>

          <div v-else>
            <div class="d-flex justify-space-between">
              <span>Teorico total</span>
              <strong>{{ formatMoney(teoricoTotal) }}</strong>
            </div>

            <v-text-field
              v-model.number="efectivoContado"
              label="Efectivo contado"
              type="number"
              min="0"
              step="0.01"
              variant="outlined"
              density="comfortable"
              class="mt-3"
            />

            <v-row dense>
              <v-col cols="12" v-for="medio in mediosCierre" :key="medio.medio">
                <v-text-field
                  v-model.number="medio.contado"
                  :label="`Contado ${medio.medio}`"
                  type="number"
                  min="0"
                  step="0.01"
                  variant="outlined"
                  density="comfortable"
                />
              </v-col>
            </v-row>

            <div class="d-flex justify-space-between mt-2">
              <span>Total contado</span>
              <strong>{{ formatMoney(totalContado) }}</strong>
            </div>
            <div class="d-flex justify-space-between text-caption">
              <span>Diferencia</span>
              <span :class="diferencia !== 0 ? 'text-error' : ''">
                {{ formatMoney(diferencia) }}
              </span>
            </div>

            <v-text-field
              v-model="motivoDiferencia"
              label="Motivo diferencia"
              variant="outlined"
              density="comfortable"
              :disabled="!needsMotivo"
              class="mt-2"
            />

            <v-btn
              color="primary"
              size="large"
              class="text-none mt-2"
              :disabled="!canClose"
              @click="dialogCerrar = true"
            >
              Cerrar caja
            </v-btn>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-dialog v-model="dialogCerrar" width="480">
      <v-card>
        <v-card-title>Cerrar caja</v-card-title>
        <v-card-text>
          Esta accion es irreversible. Queres cerrar la sesion ahora?
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn variant="text" @click="dialogCerrar = false">Cancelar</v-btn>
          <v-btn color="primary" :loading="closeLoading" @click="cerrarCaja">Confirmar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-card v-if="cierreResult" class="pos-card pa-4 mt-4">
      <div class="d-flex align-center justify-space-between">
        <div>
          <div class="text-h6">Resumen final</div>
          <div class="text-caption text-medium-emphasis">Ticket imprimible</div>
        </div>
        <v-btn variant="tonal" color="primary" class="text-none" @click="printTicket">
          Imprimir
        </v-btn>
      </div>

      <div class="ticket mt-3">
        <div class="ticket-line">Caja: {{ shortId(cierreResult.cajaId) }}</div>
        <div class="ticket-line">Sesion: {{ shortId(cierreResult.sesionId) }}</div>
        <div class="ticket-line">Fecha cierre: {{ formatDate(cierreResult.cierreAt) }}</div>
        <div class="ticket-line">Estado: {{ cierreResult.estado }}</div>
        <div class="ticket-line">Total teorico: {{ formatMoney(cierreResult.totalTeorico) }}</div>
        <div class="ticket-line">Total contado: {{ formatMoney(cierreResult.totalContado) }}</div>
        <div class="ticket-line">Diferencia: {{ formatMoney(cierreResult.diferencia) }}</div>
        <div class="ticket-line mt-2">Medios:</div>
        <div
          v-for="medio in cierreResult.medios"
          :key="medio.medio"
          class="ticket-line"
        >
          {{ medio.medio }} - T: {{ formatMoney(medio.teorico) }} / C: {{ formatMoney(medio.contado) }}
        </div>
      </div>
    </v-card>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" location="top end" timeout="1800">
      <div class="d-flex align-center gap-2">
        <v-icon>{{ snackbar.icon }}</v-icon>
        <span>{{ snackbar.text }}</span>
      </div>
    </v-snackbar>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { getJson, postJson } from '../services/apiClient';

const auth = useAuthStore();

const session = ref(null);
const resumen = ref(null);
const cierreResult = ref(null);

const cajaId = ref('');
const montoInicial = ref(0);

const movimiento = ref({
  tipo: 'Retiro',
  medioPago: 'EFECTIVO',
  motivo: '',
  monto: 0
});

const movimientosLocal = ref([]);

const efectivoContado = ref(0);
const mediosCierre = ref([
  { medio: 'TARJETA', contado: 0 },
  { medio: 'TRANSFERENCIA', contado: 0 },
  { medio: 'OTRO', contado: 0 }
]);
const motivoDiferencia = ref('');

const dialogCerrar = ref(false);
const openLoading = ref(false);
const movLoading = ref(false);
const closeLoading = ref(false);
const resumenLoading = ref(false);

const snackbar = ref({
  show: false,
  text: '',
  color: 'success',
  icon: 'mdi-check-circle'
});

const tiposMovimiento = ['Retiro', 'Gasto', 'Ajuste'];
const mediosPago = ['EFECTIVO', 'TARJETA', 'TRANSFERENCIA', 'OTRO'];

const isAbierta = computed(() => session.value?.estado === 'ABIERTA');

const teoricoTotal = computed(() => resumen.value?.saldoActual || 0);
const totalContado = computed(() => {
  const otros = mediosCierre.value.reduce((acc, medio) => acc + (medio.contado || 0), 0);
  return (efectivoContado.value || 0) + otros;
});
const diferencia = computed(() => totalContado.value - teoricoTotal.value);
const needsMotivo = computed(() => Math.abs(diferencia.value) > 0.0001);

const canClose = computed(() => {
  if (!isAbierta.value || closeLoading.value) return false;
  if (!resumen.value) return false;
  if (needsMotivo.value && !motivoDiferencia.value.trim()) return false;
  return true;
});

const resumenMedios = computed(() => {
  const totals = {};
  movimientosLocal.value.forEach((mov) => {
    const key = mov.medioPago || 'EFECTIVO';
    totals[key] = (totals[key] || 0) + mov.monto;
  });

  const medioList = [...new Set(['EFECTIVO', ...mediosPago, ...Object.keys(totals)])];
  const otros = medioList
    .filter((medio) => medio !== 'EFECTIVO')
    .reduce((acc, medio) => acc + (totals[medio] || 0), 0);

  const efectivoTeorico = teoricoTotal.value - otros;

  return medioList.map((medio) => ({
    medio,
    total: totals[medio] || 0,
    teorico: medio === 'EFECTIVO' ? efectivoTeorico : totals[medio] || 0
  }));
});

const shortId = (value) => {
  if (!value) return 'n/a';
  return value.length > 8 ? value.slice(0, 8) : value;
};

const formatMoney = (value) =>
  new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(value || 0);

const formatDate = (value) => {
  if (!value) return '-';
  try {
    return new Date(value).toLocaleString('es-AR');
  } catch {
    return value;
  }
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

const saveSession = () => {
  if (!session.value) return;
  localStorage.setItem('pos-caja-session', JSON.stringify(session.value));
};

const clearSession = () => {
  localStorage.removeItem('pos-caja-session');
};

const loadSession = () => {
  const raw = localStorage.getItem('pos-caja-session');
  if (!raw) return;
  try {
    session.value = JSON.parse(raw);
    cajaId.value = session.value.cajaId || '';
    montoInicial.value = session.value.montoInicial || 0;
  } catch {
    clearSession();
  }
};

const cargarResumen = async () => {
  if (!session.value) return;
  resumenLoading.value = true;
  try {
    const { response, data } = await getJson(`/api/v1/caja/sesiones/${session.value.id}/resumen`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    resumen.value = data;
  } catch (err) {
    flash('error', err?.message || 'No se pudo cargar el resumen.');
  } finally {
    resumenLoading.value = false;
  }
};

const abrirCaja = async () => {
  if (openLoading.value || isAbierta.value) return;
  openLoading.value = true;
  try {
    const payload = {
      cajaId: cajaId.value,
      montoInicial: Number(montoInicial.value || 0)
    };

    const { response, data } = await postJson('/api/v1/caja/sesiones/abrir', payload);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    session.value = data;
    saveSession();
    movimientosLocal.value = [];
    cierreResult.value = null;
    motivoDiferencia.value = '';
    efectivoContado.value = 0;
    mediosCierre.value.forEach((medio) => {
      medio.contado = 0;
    });
    await cargarResumen();
    flash('success', 'Caja abierta');
  } catch (err) {
    flash('error', err?.message || 'No se pudo abrir la caja.');
  } finally {
    openLoading.value = false;
  }
};

const registrarMovimiento = async () => {
  if (!session.value || !isAbierta.value || movLoading.value) return;
  movLoading.value = true;
  try {
    const payload = {
      tipo: movimiento.value.tipo,
      motivo: movimiento.value.motivo,
      monto: Number(movimiento.value.monto || 0),
      medioPago: movimiento.value.medioPago
    };

    const { response, data } = await postJson(`/api/v1/caja/sesiones/${session.value.id}/movimientos`, payload);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    movimientosLocal.value.unshift({
      id: data.id,
      tipo: data.tipo,
      medioPago: data.medioPago || movimiento.value.medioPago,
      monto: data.monto,
      motivo: data.motivo,
      fecha: data.fecha
    });

    movimiento.value = {
      tipo: 'Retiro',
      medioPago: 'EFECTIVO',
      motivo: '',
      monto: 0
    };

    await cargarResumen();
    flash('success', 'Movimiento registrado');
  } catch (err) {
    flash('error', err?.message || 'No se pudo registrar el movimiento.');
  } finally {
    movLoading.value = false;
  }
};

const cerrarCaja = async () => {
  if (!session.value || !canClose.value || closeLoading.value) return;
  closeLoading.value = true;
  try {
    const medios = mediosCierre.value
      .filter((medio) => medio.contado > 0)
      .map((medio) => ({ medio: medio.medio, contado: medio.contado }));

    const payload = {
      efectivoContado: Number(efectivoContado.value || 0),
      medios,
      motivoDiferencia: needsMotivo.value ? motivoDiferencia.value.trim() : null
    };

    const { response, data } = await postJson(`/api/v1/caja/sesiones/${session.value.id}/cerrar`, payload);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    cierreResult.value = data;
    session.value = { ...session.value, estado: 'CERRADA' };
    resumen.value = null;
    movimientosLocal.value = [];
    dialogCerrar.value = false;
    clearSession();
    flash('success', 'Caja cerrada');
  } catch (err) {
    flash('error', err?.message || 'No se pudo cerrar la caja.');
  } finally {
    closeLoading.value = false;
  }
};

const printTicket = () => {
  window.print();
};

onMounted(() => {
  loadSession();
  if (session.value) {
    cargarResumen();
  }
});
</script>

<style scoped>
.caja-page {
  animation: fade-in 0.3s ease;
}

.ticket {
  border: 1px dashed rgba(15, 23, 42, 0.2);
  padding: 12px;
  border-radius: 10px;
  font-family: 'IBM Plex Sans', sans-serif;
}

.ticket-line {
  font-size: 0.9rem;
  margin-bottom: 4px;
}

.text-error {
  color: #dc2626;
}

@media print {
  .caja-page :deep(.pos-card),
  .caja-page :deep(.v-app-bar),
  .caja-page :deep(.v-navigation-drawer) {
    display: none !important;
  }

  .ticket {
    border: none;
  }
}
</style>
