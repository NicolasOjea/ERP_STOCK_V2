<template>
  <div class="pos-page">
    <v-card class="pos-card pa-4 mb-4">
      <div class="d-flex flex-wrap align-center gap-3">
        <div>
          <div class="text-h6">POS</div>
          <div class="text-caption text-medium-emphasis">Venta activa</div>
        </div>
        <v-chip class="status-chip" color="secondary" variant="tonal">
          {{ ventaEstado }}
        </v-chip>
        <v-chip
          class="status-chip"
          :color="cajaStatus === 'ABIERTA' ? 'success' : 'error'"
          variant="tonal"
        >
          Caja {{ cajaStatus }}
        </v-chip>
        <v-spacer />
        <div class="d-flex align-center gap-3 text-caption text-medium-emphasis">
          <div class="d-flex align-center gap-1">
            <v-icon size="16">mdi-account</v-icon>
            <span>Usuario: {{ shortId(auth.userId) }}</span>
          </div>
          <div class="d-flex align-center gap-1">
            <v-icon size="16">mdi-storefront</v-icon>
            <span>Sucursal: {{ shortId(auth.sucursalId) }}</span>
          </div>
        </div>
      </div>

      <div class="mt-4 d-flex flex-wrap align-center gap-3">
        <v-text-field
          ref="scanInputRef"
          v-model="scanInput"
          class="scan-input pos-scan"
          :class="scanFlashClass"
          label="Escanear codigo o SKU"
          variant="outlined"
          density="comfortable"
          prepend-inner-icon="mdi-barcode-scan"
          hide-details
          @keyup.enter="handleScan"
          @blur="maybeRestoreScan"
          :disabled="!canScan || scanLoading"
        />
        <v-autocomplete
          v-model="productoSeleccionado"
          v-model:search="productoSearch"
          :items="productosEncontrados"
          item-title="label"
          return-object
          label="Buscar producto"
          variant="outlined"
          density="comfortable"
          clearable
          hide-details
          class="pos-search"
          :loading="productoSearchLoading"
          :disabled="!cajaAbierta"
          @update:search="searchProductos"
          @update:modelValue="onProductoSeleccionado"
        />
        <v-btn
          color="primary"
          size="large"
          class="text-none"
          :loading="scanLoading"
          :disabled="!canScan || scanLoading"
          @click="handleScan"
        >
          Agregar
        </v-btn>
        <v-btn
          color="secondary"
          variant="tonal"
          size="large"
          class="text-none"
          :loading="creatingVenta"
          @click="createVenta"
        >
          Nueva venta
        </v-btn>
      </div>

      <div class="hotkeys mt-3">
        <span class="hotkey">F2 Nueva venta</span>
        <span class="hotkey">F8 Confirmar</span>
        <span class="hotkey">Esc Anular</span>
      </div>
    </v-card>

    <v-row dense>
      <v-col cols="12" md="8">
        <v-card class="pos-card pa-4">
          <div class="d-flex align-center justify-space-between">
            <div>
              <div class="text-h6">Carrito</div>
              <div class="text-caption text-medium-emphasis">
                {{ items.length }} items - Lista {{ venta?.listaPrecio || 'Minorista' }}
              </div>
            </div>
            <v-text-field
              v-model="tableSearch"
              label="Busqueda rapida"
              variant="outlined"
              density="compact"
              prepend-inner-icon="mdi-magnify"
              hide-details
              style="max-width: 220px"
            />
          </div>

          <v-data-table
            :headers="headers"
            :items="items"
            :search="tableSearch"
            item-key="id"
            density="compact"
            class="mt-3"
            height="420"
          >
            <template #item.cantidad="{ item }">
              <v-text-field
                v-model.number="qtyEdits[getRow(item).id]"
                type="number"
                min="1"
                step="1"
                density="compact"
                hide-details
                variant="outlined"
                style="max-width: 90px"
                :disabled="!canEdit"
                @blur="commitQty(getRow(item))"
                @keyup.enter="commitQty(getRow(item))"
              />
            </template>
            <template #item.precioUnitario="{ item }">
              {{ formatMoney(getRow(item).precioUnitario) }}
            </template>
            <template #item.subtotal="{ item }">
              <strong>{{ formatMoney(getRow(item).subtotal) }}</strong>
            </template>
            <template #item.acciones="{ item }">
              <v-btn
                icon="mdi-delete"
                variant="text"
                color="error"
                :disabled="!canEdit"
                @click="removeItem(getRow(item))"
              />
            </template>
          </v-data-table>

          <div v-if="!items.length" class="text-caption text-medium-emphasis mt-3">
            Escanea un codigo para comenzar la venta.
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="pos-card pa-4">
          <div class="text-h6">Totales</div>
          <div class="text-caption text-medium-emphasis">Resumen actual</div>
          <v-divider class="my-3" />

          <div class="d-flex justify-space-between mb-2">
            <span>Total bruto</span>
            <strong>{{ formatMoney(totalBruto) }}</strong>
          </div>
          <div class="d-flex justify-space-between mb-2">
            <span>Descuento</span>
            <strong>- {{ formatMoney(totalDescuento) }}</strong>
          </div>
          <div class="d-flex justify-space-between text-h6">
            <span>Total neto</span>
            <strong>{{ formatMoney(totalNeto) }}</strong>
          </div>
          <div class="text-caption text-medium-emphasis mt-2">
            Items: {{ totalItems }}
          </div>

          <v-divider class="my-3" />

          <v-btn
            color="primary"
            size="large"
            class="text-none"
            block
            :disabled="!canEdit || !items.length"
            @click="openPagos"
          >
            Cobrar
          </v-btn>
          <v-btn
            color="primary"
            variant="tonal"
            class="text-none mt-2"
            block
            :loading="recalcularLoading"
            :disabled="!canEdit || !items.length"
            @click="recalcularPromos"
          >
            Recalcular promos
          </v-btn>
          <v-btn
            color="error"
            variant="tonal"
            class="text-none mt-2"
            block
            :disabled="!ventaId"
            @click="dialogAnular = true"
          >
            Anular / Cancelar
          </v-btn>
        </v-card>
      </v-col>
    </v-row>

    <v-dialog v-model="dialogPagos" width="640">
      <v-card>
        <v-card-title>Pagos</v-card-title>
        <v-card-text>
          <div class="d-flex justify-space-between text-body-2 mb-3">
            <span>Total a cobrar</span>
            <strong>{{ formatMoney(totalNeto) }}</strong>
          </div>

          <v-row v-for="(line, index) in pagos" :key="line.id" dense class="mb-2">
            <v-col cols="12" md="4">
              <v-select
                v-model="line.medioPago"
                :items="mediosPago"
                label="Medio"
                density="compact"
                variant="outlined"
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model.number="line.monto"
                type="number"
                min="0"
                step="0.01"
                label="Monto"
                density="compact"
                variant="outlined"
              />
            </v-col>
            <v-col cols="12" md="4" v-if="line.medioPago === 'EFECTIVO'">
              <v-text-field
                v-model.number="line.recibido"
                type="number"
                min="0"
                step="0.01"
                label="Recibido"
                density="compact"
                variant="outlined"
              />
              <div class="text-caption text-medium-emphasis mt-1">
                Vuelto: {{ formatMoney(vuelto(line)) }}
              </div>
            </v-col>
            <v-col cols="12" md="4" v-else>
              <v-btn
                icon="mdi-close"
                variant="text"
                color="error"
                class="mt-1"
                @click="removePago(index)"
              />
            </v-col>
          </v-row>

          <v-btn variant="tonal" color="primary" class="text-none" @click="addPago">
            Agregar medio
          </v-btn>

          <v-divider class="my-3" />
          <div class="d-flex justify-space-between">
            <span>Total pagos</span>
            <strong>{{ formatMoney(totalPagos) }}</strong>
          </div>
          <div class="d-flex justify-space-between text-caption text-medium-emphasis">
            <span>Diferencia</span>
            <span>{{ formatMoney(diferenciaPagos) }}</span>
          </div>
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn variant="text" @click="dialogPagos = false">Cancelar</v-btn>
          <v-btn
            color="primary"
            :loading="confirmLoading"
            :disabled="!canConfirm"
            @click="confirmarVenta"
          >
            Confirmar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="dialogAnular" width="480">
      <v-card>
        <v-card-title>Anular / Cancelar venta</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="motivoAnular"
            label="Motivo"
            variant="outlined"
            density="comfortable"
          />
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn variant="text" @click="dialogAnular = false">Cancelar</v-btn>
          <v-btn color="error" :loading="anularLoading" @click="anularVenta">
            Anular
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="dialogStock" width="520">
      <v-card>
        <v-card-title>Stock insuficiente</v-card-title>
        <v-card-text>
          <div class="text-body-2 mb-2">Revisar los siguientes items:</div>
          <v-list density="compact">
            <v-list-item v-for="item in stockFaltantes" :key="item.id">
              <v-list-item-title>{{ item.nombre }}</v-list-item-title>
              <v-list-item-subtitle>
                Cantidad: {{ item.cantidad }}
              </v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn color="primary" variant="text" @click="dialogStock = false">Ok</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" location="top end" timeout="1600">
      <div class="d-flex align-center gap-2">
        <v-icon>{{ snackbar.icon }}</v-icon>
        <span>{{ snackbar.text }}</span>
      </div>
    </v-snackbar>
  </div>
</template>

<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useAuthStore } from '../stores/auth';
import { getJson, postJson, requestJson } from '../services/apiClient';

const auth = useAuthStore();

const venta = ref(null);
const items = ref([]);
const pricing = ref(null);
const cajaStatus = ref('CERRADA');
const cajaSessionId = ref('');
const scanInput = ref('');
const scanInputRef = ref(null);
const scanLoading = ref(false);
const creatingVenta = ref(false);
const confirmLoading = ref(false);
const recalcularLoading = ref(false);
const anularLoading = ref(false);
const tableSearch = ref('');
const dialogPagos = ref(false);
const dialogAnular = ref(false);
const dialogStock = ref(false);
const motivoAnular = ref('');
const qtyEdits = ref({});
const productoSearch = ref('');
const productoSearchLoading = ref(false);
const productosEncontrados = ref([]);
const productoSeleccionado = ref(null);
const POS_VENTA_KEY = 'pos-venta-id';
let productoSearchTimer = null;

const snackbar = ref({
  show: false,
  text: '',
  color: 'success',
  icon: 'mdi-check-circle'
});

const pagos = ref([]);
const mediosPago = ['EFECTIVO', 'TARJETA', 'TRANSFERENCIA', 'OTRO'];

const headers = [
  { title: 'Producto', value: 'nombre' },
  { title: 'SKU', value: 'sku' },
  { title: 'Codigo', value: 'codigo' },
  { title: 'Cant', value: 'cantidad', align: 'end' },
  { title: 'Precio', value: 'precioUnitario', align: 'end' },
  { title: 'Subtotal', value: 'subtotal', align: 'end' },
  { title: '', value: 'acciones', align: 'end' }
];

const ventaId = computed(() => venta.value?.id || '');
const ventaEstado = computed(() => venta.value?.estado || 'SIN_VENTA');
const canEdit = computed(() => ventaEstado.value === 'BORRADOR');
const cajaAbierta = computed(() => cajaStatus.value === 'ABIERTA');
const canScan = computed(() => cajaAbierta.value && (ventaEstado.value === 'BORRADOR' || ventaEstado.value === 'SIN_VENTA'));

const totalBruto = computed(() => {
  if (pricing.value?.totalBruto != null) return pricing.value.totalBruto;
  return items.value.reduce((acc, item) => acc + item.subtotal, 0);
});

const totalDescuento = computed(() => {
  if (pricing.value?.totalDescuento != null) return pricing.value.totalDescuento;
  return 0;
});

const totalNeto = computed(() => {
  if (pricing.value?.totalNeto != null) return pricing.value.totalNeto;
  return items.value.reduce((acc, item) => acc + item.subtotal, 0);
});

const totalItems = computed(() => items.value.reduce((acc, item) => acc + item.cantidad, 0));

const totalPagos = computed(() => pagos.value.reduce((acc, line) => acc + (line.monto || 0), 0));
const diferenciaPagos = computed(() => totalPagos.value - totalNeto.value);

const canConfirm = computed(() => {
  if (!canEdit.value || !items.value.length) return false;
  if (totalNeto.value <= 0) return false;
  if (Math.abs(diferenciaPagos.value) > 0.0001) return false;
  return true;
});

const loadCajaSession = () => {
  const raw = localStorage.getItem('pos-caja-session');
  if (!raw) {
    cajaStatus.value = 'CERRADA';
    cajaSessionId.value = '';
    return;
  }
  try {
    const session = JSON.parse(raw);
    if (session?.estado === 'ABIERTA') {
      cajaStatus.value = 'ABIERTA';
      cajaSessionId.value = session.id || '';
    } else {
      cajaStatus.value = 'CERRADA';
      cajaSessionId.value = '';
    }
  } catch {
    cajaStatus.value = 'CERRADA';
    cajaSessionId.value = '';
  }
};

const saveVentaId = (id) => {
  if (!id) return;
  localStorage.setItem(POS_VENTA_KEY, id);
};

const clearVentaId = () => {
  localStorage.removeItem(POS_VENTA_KEY);
};

const scanFlash = ref('');
const scanFlashClass = computed(() => {
  if (scanFlash.value === 'success') return 'scan-flash-success';
  if (scanFlash.value === 'error') return 'scan-flash-error';
  return '';
});

const stockFaltantes = computed(() => items.value.map((item) => ({
  id: item.id,
  nombre: item.nombre,
  cantidad: item.cantidad
})));

const shortId = (value) => {
  if (!value) return 'n/a';
  return value.length > 8 ? value.slice(0, 8) : value;
};

const getRow = (row) => row?.raw ?? row;

const formatMoney = (value) =>
  new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(value || 0);

const focusScan = () => {
  nextTick(() => {
    const el = scanInputRef.value?.$el?.querySelector('input');
    if (el && !el.disabled) el.focus();
  });
};

const maybeRestoreScan = () => {
  setTimeout(() => {
    const activeTag = document.activeElement?.tagName || '';
    if (activeTag === 'INPUT' || activeTag === 'TEXTAREA') return;
    focusScan();
  }, 0);
};

const flash = (type, text) => {
  snackbar.value = {
    show: true,
    text,
    color: type === 'success' ? 'success' : 'error',
    icon: type === 'success' ? 'mdi-check-circle' : 'mdi-alert-circle'
  };
  scanFlash.value = type;
  setTimeout(() => {
    scanFlash.value = '';
  }, 250);
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

const ensureVenta = async () => {
  if (ventaId.value) return ventaId.value;
  await createVenta();
  return ventaId.value;
};

const applyItemDto = (dto) => {
  const index = items.value.findIndex((item) => item.id === dto.id);
  const item = {
    id: dto.id,
    productoId: dto.productoId,
    nombre: dto.nombre,
    sku: dto.sku,
    codigo: dto.codigo,
    cantidad: dto.cantidad,
    precioUnitario: dto.precioUnitario,
    subtotal: dto.subtotal ?? dto.cantidad * dto.precioUnitario
  };

  if (index >= 0) {
    items.value.splice(index, 1, item);
  } else {
    items.value.unshift(item);
  }
  qtyEdits.value[item.id] = item.cantidad;
  pricing.value = null;
};

const createVenta = async () => {
  if (creatingVenta.value) return;
  creatingVenta.value = true;
  try {
    const { response, data } = await postJson('/api/v1/ventas', {});
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    venta.value = data;
    saveVentaId(venta.value?.id);
    items.value = data.items?.map((item) => ({
      ...item,
      subtotal: item.subtotal ?? item.cantidad * item.precioUnitario
    })) || [];
    qtyEdits.value = {};
    items.value.forEach((item) => {
      qtyEdits.value[item.id] = item.cantidad;
    });
    pricing.value = null;
    pagos.value = [];
    flash('success', 'Venta creada');
  } catch (err) {
    flash('error', err?.message || 'No se pudo crear la venta.');
  } finally {
    creatingVenta.value = false;
    focusScan();
  }
};

const restoreVenta = async () => {
  const savedId = localStorage.getItem(POS_VENTA_KEY);
  if (!savedId) return;

  try {
    const { response, data } = await getJson(`/api/v1/ventas/${savedId}`);
    if (!response.ok) {
      clearVentaId();
      return;
    }
    venta.value = data;
    items.value = data.items?.map((item) => ({
      ...item,
      subtotal: item.subtotal ?? item.cantidad * item.precioUnitario
    })) || [];
    qtyEdits.value = {};
    items.value.forEach((item) => {
      qtyEdits.value[item.id] = item.cantidad;
    });
    pricing.value = null;
    pagos.value = [];
  } catch {
    clearVentaId();
  }
};

const handleScan = async () => {
  const code = scanInput.value.trim();
  if (!code || scanLoading.value || !canScan.value) return;
  if (!cajaAbierta.value) {
    flash('error', 'Caja cerrada');
    return;
  }

  const id = await ensureVenta();
  if (!id) return;

  scanLoading.value = true;
  scanInput.value = '';

  try {
    const { response, data } = await postJson(`/api/v1/ventas/${id}/items/scan`, { code });
    if (!response.ok) {
      const message = extractProblemMessage(data);
      if (response.status === 404) {
        flash('error', 'Codigo no encontrado');
      } else {
        flash('error', message);
      }
      return;
    }

    applyItemDto(data);
    flash('success', `Agregado: ${data.nombre}`);
  } catch (err) {
    flash('error', err?.message || 'Error al escanear.');
  } finally {
    scanLoading.value = false;
    focusScan();
  }
};

const mapProductoResults = (items) =>
  (items || []).map((item) => ({
    ...item,
    label: `${item.name} (${item.sku})${item.codigo ? ` - ${item.codigo}` : ''}`
  }));

const searchProductos = (term) => {
  if (!cajaAbierta.value) {
    productosEncontrados.value = [];
    return;
  }
  if (productoSearchTimer) {
    clearTimeout(productoSearchTimer);
  }
  if (!term || term.trim().length < 2) {
    productosEncontrados.value = [];
    return;
  }
  productoSearchTimer = setTimeout(async () => {
    productoSearchLoading.value = true;
    try {
      const params = new URLSearchParams();
      params.set('search', term.trim());
      params.set('activo', 'true');
      const { response, data } = await getJson(`/api/v1/productos?${params.toString()}`);
      if (!response.ok) {
        throw new Error(extractProblemMessage(data));
      }
      productosEncontrados.value = mapProductoResults(data || []);
    } catch (err) {
      flash('error', err?.message || 'No se pudieron buscar productos.');
    } finally {
      productoSearchLoading.value = false;
    }
  }, 250);
};

const onProductoSeleccionado = async (producto) => {
  if (!producto?.id) return;
  if (!cajaAbierta.value) {
    flash('error', 'Caja cerrada');
    return;
  }
  const id = await ensureVenta();
  if (!id) return;

  try {
    const { response, data } = await postJson(`/api/v1/ventas/${id}/items`, {
      productId: producto.id
    });
    if (!response.ok) {
      const message = extractProblemMessage(data);
      flash('error', message);
      return;
    }
    applyItemDto(data);
    flash('success', `Agregado: ${data.nombre}`);
  } catch (err) {
    flash('error', err?.message || 'No se pudo agregar el producto.');
  } finally {
    productoSeleccionado.value = null;
    productoSearch.value = '';
    productosEncontrados.value = [];
    focusScan();
  }
};

const removeItem = async (item) => {
  if (!ventaId.value || !canEdit.value) return;
  try {
    const { response, data } = await requestJson(`/api/v1/ventas/${ventaId.value}/items/${item.id}`, {
      method: 'DELETE'
    });
    if (!response.ok) {
      if (response.status === 404 || response.status === 405) {
        const fallback = await requestJson(`/api/v1/ventas/${ventaId.value}/items/${item.id}`, {
          method: 'PATCH',
          body: JSON.stringify({ cantidad: 0 })
        });
        if (!fallback.response.ok) {
          const message = extractProblemMessage(fallback.data);
          flash('error', message);
          return;
        }
      } else {
        const message = extractProblemMessage(data);
        flash('error', message);
        return;
      }
    }
    items.value = items.value.filter((i) => i.id !== item.id);
    delete qtyEdits.value[item.id];
    flash('success', 'Item eliminado');
  } catch (err) {
    flash('error', err?.message || 'No se pudo eliminar el item.');
  }
};

const commitQty = async (item) => {
  const cantidad = Number(qtyEdits.value[item.id]);
  if (!ventaId.value || !canEdit.value) return;
  if (!cantidad || cantidad <= 0 || Number.isNaN(cantidad)) {
    qtyEdits.value[item.id] = item.cantidad;
    flash('error', 'Cantidad invalida');
    return;
  }
  if (cantidad === item.cantidad) return;

  try {
    const { response, data } = await requestJson(`/api/v1/ventas/${ventaId.value}/items/${item.id}`, {
      method: 'PATCH',
      body: JSON.stringify({ cantidad })
    });

    if (!response.ok) {
      const message = extractProblemMessage(data);
      qtyEdits.value[item.id] = item.cantidad;
      flash('error', message);
      return;
    }

    applyItemDto(data);
  } catch (err) {
    qtyEdits.value[item.id] = item.cantidad;
    flash('error', err?.message || 'Error al actualizar item.');
  }
};

const recalcularPromos = async () => {
  if (!ventaId.value || recalcularLoading.value) return;
  recalcularLoading.value = true;
  try {
    const { response, data } = await postJson(`/api/v1/ventas/${ventaId.value}/recalcular`, {});
    if (!response.ok) {
      if (response.status === 404) {
        flash('error', 'Recalculo no disponible.');
        return;
      }
      throw new Error(extractProblemMessage(data));
    }
    pricing.value = data;
    flash('success', 'Promos recalculadas');
  } catch (err) {
    flash('error', err?.message || 'Error al recalcular.');
  } finally {
    recalcularLoading.value = false;
  }
};

const openPagos = () => {
  dialogPagos.value = true;
  if (!pagos.value.length) {
    pagos.value = [
      {
        id: `${Date.now()}-${Math.random()}`,
        medioPago: 'EFECTIVO',
        monto: totalNeto.value,
        recibido: totalNeto.value
      }
    ];
  }
};

const addPago = () => {
  pagos.value.push({
    id: `${Date.now()}-${Math.random()}`,
    medioPago: 'TARJETA',
    monto: 0,
    recibido: 0
  });
};

const removePago = (index) => {
  pagos.value.splice(index, 1);
};

const vuelto = (line) => {
  if (line.medioPago !== 'EFECTIVO') return 0;
  const recibido = Number(line.recibido || 0);
  const monto = Number(line.monto || 0);
  return recibido > monto ? recibido - monto : 0;
};

const confirmarVenta = async () => {
  if (!ventaId.value || confirmLoading.value) return;
  confirmLoading.value = true;

  try {
    loadCajaSession();
    const payload = {
      pagos: pagos.value
        .filter((line) => line.medioPago && line.monto > 0)
        .map((line) => ({ medioPago: line.medioPago, monto: line.monto })),
      cajaSesionId: cajaSessionId.value || null
    };

    const { response, data } = await postJson(`/api/v1/ventas/${ventaId.value}/confirmar`, payload);
    if (!response.ok) {
      const message = extractProblemMessage(data);
      if (message.toLowerCase().includes('stock insuficiente')) {
        dialogStock.value = true;
      } else if (message.toLowerCase().includes('caja')) {
        cajaStatus.value = 'CERRADA';
      }
      throw new Error(message);
    }

    venta.value = data.venta || data.Venta || data;
    clearVentaId();
    items.value = (venta.value.items || []).map((item) => ({
      ...item,
      subtotal: item.subtotal ?? item.cantidad * item.precioUnitario
    }));
    qtyEdits.value = {};
    items.value.forEach((item) => {
      qtyEdits.value[item.id] = item.cantidad;
    });
    pricing.value = null;
    cajaStatus.value = 'ABIERTA';
    dialogPagos.value = false;
    pagos.value = [];
    flash('success', 'Venta confirmada');
  } catch (err) {
    flash('error', err?.message || 'Error al confirmar venta.');
  } finally {
    confirmLoading.value = false;
  }
};

const anularVenta = async () => {
  if (!ventaId.value || anularLoading.value) return;
  if (!motivoAnular.value.trim()) {
    flash('error', 'Motivo obligatorio');
    return;
  }

  anularLoading.value = true;
  try {
    const { response, data } = await postJson(`/api/v1/ventas/${ventaId.value}/anular`, {
      motivo: motivoAnular.value.trim()
    });
    if (!response.ok) {
      const message = extractProblemMessage(data);
      if (message.toLowerCase().includes('caja')) {
        cajaStatus.value = 'CERRADA';
      }
      throw new Error(message);
    }

    venta.value = data.venta || data.Venta || data;
    clearVentaId();
    items.value = (venta.value.items || []).map((item) => ({
      ...item,
      subtotal: item.subtotal ?? item.cantidad * item.precioUnitario
    }));
    qtyEdits.value = {};
    items.value.forEach((item) => {
      qtyEdits.value[item.id] = item.cantidad;
    });
    pricing.value = null;
    dialogAnular.value = false;
    motivoAnular.value = '';
    flash('error', 'Venta anulada');
  } catch (err) {
    flash('error', err?.message || 'Error al anular venta.');
  } finally {
    anularLoading.value = false;
  }
};

const onKeydown = (event) => {
  const activeTag = document.activeElement?.tagName || '';
  const isInput = activeTag === 'INPUT' || activeTag === 'TEXTAREA';
  const isShortcutKey = event.ctrlKey || event.metaKey || event.altKey;
  if (!isInput && !isShortcutKey && event.key.length === 1) {
    focusScan();
  }

  if (event.key === 'F2') {
    event.preventDefault();
    createVenta();
  }
  if (event.key === 'F8') {
    event.preventDefault();
    if (items.value.length && canEdit.value) {
      openPagos();
    }
  }
  if (event.key === 'Escape') {
    event.preventDefault();
    if (ventaId.value) {
      dialogAnular.value = true;
    }
  }
};

watch(dialogPagos, (open) => {
  if (!open) {
    focusScan();
  }
});

watch(dialogAnular, (open) => {
  if (!open) {
    focusScan();
  }
});

onMounted(() => {
  loadCajaSession();
  restoreVenta();
  focusScan();
  window.addEventListener('keydown', onKeydown);
});

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown);
});
</script>

<style scoped>
.pos-page {
  animation: fade-in 0.3s ease;
}

.pos-scan {
  min-width: 320px;
}

.pos-search {
  min-width: 280px;
}

.gap-1 {
  gap: 4px;
}

.gap-2 {
  gap: 8px;
}

.gap-3 {
  gap: 12px;
}
</style>
