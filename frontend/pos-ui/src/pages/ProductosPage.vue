<template>
  <div class="productos-page">
    <v-tabs v-model="tab" color="primary" class="mb-4">
      <v-tab value="productos">Productos</v-tab>
      <v-tab value="proveedores">Proveedores</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item value="productos">
        <v-card class="pos-card pa-4 mb-4">
          <div class="d-flex flex-wrap align-center gap-3">
            <div>
              <div class="text-h6">Productos</div>
              <div class="text-caption text-medium-emphasis">ABM + stock + codigos</div>
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
              @keyup.enter="loadProducts"
            />
            <v-text-field
              v-model="scanInput"
              label="Escanear codigo"
              variant="outlined"
              density="comfortable"
              hide-details
              style="min-width: 220px"
              @keyup.enter="handleScan"
            />
            <v-btn-toggle v-model="activoFilter" density="comfortable" mandatory class="ml-2">
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
                <template #item.precioBase="{ item }">
                  {{ formatMoney(item.precioBase) }}
                </template>
                <template #item.precioVenta="{ item }">
                  {{ formatMoney(item.precioVenta) }}
                </template>
                <template #item.isActive="{ item }">
                  <v-chip size="small" :color="item.isActive ? 'success' : 'error'" variant="tonal">
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
                <v-autocomplete
                  v-model="form.proveedorId"
                  :items="proveedoresLookup"
                  :loading="proveedorLoading"
                  label="Proveedor"
                  item-title="name"
                  item-value="id"
                  variant="outlined"
                  density="comfortable"
                  clearable
                  :error-messages="errors.proveedorId"
                  @update:search="searchProveedores"
                  @blur="validateField('proveedorId')"
                  required
                />
                <v-text-field
                  v-model="form.precioBase"
                  label="Precio base"
                  variant="outlined"
                  density="comfortable"
                  :error-messages="errors.precioBase"
                  @blur="validateField('precioBase')"
                />
                <v-text-field
                  v-model="form.margenPct"
                  label="% Ganancia"
                  variant="outlined"
                  density="comfortable"
                  type="number"
                  min="0"
                  step="0.01"
                  :disabled="form.precioManual"
                  :error-messages="errors.margenPct"
                  @blur="validateField('margenPct')"
                />
                <v-checkbox
                  v-model="form.precioManual"
                  label="Precio manual"
                  color="primary"
                  hide-details
                />
                <v-text-field
                  v-if="form.precioManual"
                  v-model="form.precioVentaManual"
                  label="Precio venta manual"
                  variant="outlined"
                  density="comfortable"
                  type="number"
                  min="0"
                  step="0.01"
                  :error-messages="errors.precioVentaManual"
                  @blur="validateField('precioVentaManual')"
                />
                <div v-else class="text-caption text-medium-emphasis">
                  Precio venta calculado: {{ formatMoney(precioVentaCalculado) }}
                </div>
                <v-text-field
                  v-model="form.codigoPrincipal"
                  label="Codigo principal"
                  variant="outlined"
                  density="comfortable"
                  hint="Se agrega al guardar"
                  persistent-hint
                />
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
                  v-model="stockConfig.stockDeseado"
                  label="Stock deseado"
                  type="number"
                  min="0"
                  step="0.01"
                  variant="outlined"
                  density="comfortable"
                  :error-messages="errors.stockDeseado"
                  @blur="validateField('stockDeseado')"
                />
                <v-text-field
                  v-model="stockConfig.toleranciaPct"
                  label="Tolerancia (%)"
                  type="number"
                  min="0"
                  step="0.01"
                  variant="outlined"
                  density="comfortable"
                  suffix="%"
                  :error-messages="errors.toleranciaPct"
                  @blur="validateField('toleranciaPct')"
                />
                <v-text-field
                  v-model="form.stockInicial"
                  label="Stock inicial"
                  type="number"
                  min="0"
                  step="0.01"
                  variant="outlined"
                  density="comfortable"
                  hint="Solo aplica al crear el producto"
                  persistent-hint
                  :disabled="!!form.id"
                  :error-messages="errors.stockInicial"
                  @blur="validateField('stockInicial')"
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

              <div class="text-subtitle-2">Codigos existentes</div>
              <div class="text-caption text-medium-emphasis">Gestion de codigos asociados</div>
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
            </v-card>
          </v-col>
        </v-row>
      </v-window-item>

      <v-window-item value="proveedores">
        <v-card class="pos-card pa-4 mb-4">
          <div class="d-flex flex-wrap align-center gap-3">
            <div>
              <div class="text-h6">Proveedores</div>
              <div class="text-caption text-medium-emphasis">Alta y gestion</div>
            </div>
            <v-spacer />
            <v-btn color="primary" class="text-none" @click="resetProveedorForm">Nuevo</v-btn>
          </div>

          <div class="mt-4 d-flex flex-wrap align-center gap-3">
            <v-text-field
              v-model="proveedorSearch"
              label="Buscar proveedor"
              variant="outlined"
              density="comfortable"
              hide-details
              style="min-width: 260px"
              @keyup.enter="loadProveedores"
            />
            <v-btn-toggle v-model="proveedorActivoFilter" density="comfortable" mandatory class="ml-2">
              <v-btn value="all" class="text-none">Todos</v-btn>
              <v-btn value="true" class="text-none">Activos</v-btn>
              <v-btn value="false" class="text-none">Inactivos</v-btn>
            </v-btn-toggle>
            <v-btn
              color="primary"
              variant="tonal"
              class="text-none"
              :loading="proveedorLoadingTable"
              @click="loadProveedores"
            >
              Buscar
            </v-btn>
          </div>
        </v-card>

        <v-row dense>
          <v-col cols="12" md="7">
            <v-card class="pos-card pa-4">
              <div class="text-h6">Listado</div>
              <div class="text-caption text-medium-emphasis">Selecciona para editar</div>
              <v-data-table
                :headers="proveedorHeaders"
                :items="proveedoresTable"
                item-key="id"
                class="mt-3"
                density="compact"
                height="520"
                @click:row="selectProveedor"
              >
                <template #item.isActive="{ item }">
                  <v-chip size="small" :color="item.isActive ? 'success' : 'error'" variant="tonal">
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
                  <div class="text-h6">{{ proveedorForm.id ? 'Editar proveedor' : 'Nuevo proveedor' }}</div>
                  <div class="text-caption text-medium-emphasis">
                    {{ proveedorForm.id ? shortId(proveedorForm.id) : 'Sin seleccionar' }}
                  </div>
                </div>
                <v-btn
                  color="primary"
                  variant="tonal"
                  class="text-none"
                  :loading="proveedorSaving"
                  @click="saveProveedor"
                >
                  Guardar
                </v-btn>
              </div>

              <v-form class="mt-3">
                <v-text-field
                  v-model="proveedorForm.name"
                  label="Nombre"
                  variant="outlined"
                  density="comfortable"
                  :error-messages="proveedorErrors.name"
                  @blur="validateProveedorField('name')"
                  required
                />
                <v-text-field
                  v-model="proveedorForm.telefono"
                  label="Telefono"
                  variant="outlined"
                  density="comfortable"
                  :error-messages="proveedorErrors.telefono"
                  @blur="validateProveedorField('telefono')"
                  required
                />
                <v-text-field
                  v-model="proveedorForm.cuit"
                  label="CUIT"
                  variant="outlined"
                  density="comfortable"
                />
                <v-text-field
                  v-model="proveedorForm.direccion"
                  label="Direccion"
                  variant="outlined"
                  density="comfortable"
                />
                <v-switch
                  :model-value="proveedorForm.isActive"
                  label="Activo"
                  color="primary"
                  inset
                  @update:model-value="(value) => (proveedorForm.isActive = value)"
                />
              </v-form>
            </v-card>
          </v-col>
        </v-row>
      </v-window-item>
    </v-window>

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
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getJson, postJson, requestJson } from '../services/apiClient';

const tab = ref('productos');
const route = useRoute();
const router = useRouter();

const products = ref([]);
const loading = ref(false);
const saving = ref(false);
const codeLoading = ref(false);

const search = ref('');
const scanInput = ref('');
const activoFilter = ref('all');

const proveedoresLookup = ref([]);
const proveedorLoading = ref(false);

const form = reactive({
  id: '',
  name: '',
  sku: '',
  proveedorId: '',
  precioBase: '',
  margenPct: '30',
  precioManual: false,
  precioVentaManual: '',
  codigoPrincipal: '',
  stockInicial: '',
  isActive: true
});

const stockConfig = reactive({
  stockMinimo: '',
  stockDeseado: '',
  toleranciaPct: '25'
});

const errors = reactive({
  name: '',
  sku: '',
  proveedorId: '',
  precioBase: '',
  margenPct: '',
  precioVentaManual: '',
  stockInicial: '',
  stockMinimo: '',
  stockDeseado: '',
  toleranciaPct: ''
});

const codes = ref([]);

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
  { title: 'Proveedor', value: 'proveedor' },
  { title: 'Precio base', value: 'precioBase' },
  { title: 'Precio venta', value: 'precioVenta' },
  { title: 'Estado', value: 'isActive' }
];

const proveedorHeaders = [
  { title: 'Nombre', value: 'name' },
  { title: 'Telefono', value: 'telefono' },
  { title: 'CUIT', value: 'cuit' },
  { title: 'Direccion', value: 'direccion' },
  { title: 'Estado', value: 'isActive' }
];

const proveedoresTable = ref([]);
const proveedorSearch = ref('');
const proveedorActivoFilter = ref('all');
const proveedorLoadingTable = ref(false);
const proveedorSaving = ref(false);

const proveedorForm = reactive({
  id: '',
  name: '',
  telefono: '',
  cuit: '',
  direccion: '',
  isActive: true
});

const proveedorErrors = reactive({
  name: '',
  telefono: ''
});

const formatMoney = (value) =>
  new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(value || 0);

const precioVentaCalculado = computed(() => {
  const base = Number(form.precioBase);
  const margen = Number(form.margenPct);
  if (Number.isNaN(base) || base <= 0) return 0;
  if (Number.isNaN(margen) || margen < 0) return base;
  return base * (1 + margen / 100);
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
  if (field === 'proveedorId') {
    errors.proveedorId = form.proveedorId ? '' : 'El proveedor es obligatorio.';
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
  if (field === 'margenPct') {
    if (form.precioManual) {
      errors.margenPct = '';
    } else if (form.margenPct === '') {
      errors.margenPct = '';
    } else if (Number(form.margenPct) < 0 || Number.isNaN(Number(form.margenPct))) {
      errors.margenPct = 'Margen invalido.';
    } else {
      errors.margenPct = '';
    }
  }
  if (field === 'precioVentaManual') {
    if (!form.precioManual) {
      errors.precioVentaManual = '';
    } else if (form.precioVentaManual === '') {
      errors.precioVentaManual = 'Precio venta obligatorio.';
    } else if (Number(form.precioVentaManual) < 0 || Number.isNaN(Number(form.precioVentaManual))) {
      errors.precioVentaManual = 'Precio venta invalido.';
    } else {
      errors.precioVentaManual = '';
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
  if (field === 'stockDeseado') {
    if (stockConfig.stockDeseado === '') {
      errors.stockDeseado = '';
    } else if (Number(stockConfig.stockDeseado) < 0 || Number.isNaN(Number(stockConfig.stockDeseado))) {
      errors.stockDeseado = 'Stock deseado invalido.';
    } else {
      errors.stockDeseado = '';
    }
  }
  if (field === 'stockInicial') {
    if (form.stockInicial === '') {
      errors.stockInicial = '';
    } else if (Number(form.stockInicial) < 0 || Number.isNaN(Number(form.stockInicial))) {
      errors.stockInicial = 'Stock inicial invalido.';
    } else {
      errors.stockInicial = '';
    }
  }
  if (field === 'toleranciaPct') {
    if (stockConfig.toleranciaPct === '') {
      errors.toleranciaPct = '';
    } else if (Number(stockConfig.toleranciaPct) < 0 || Number.isNaN(Number(stockConfig.toleranciaPct))) {
      errors.toleranciaPct = 'Tolerancia invalida.';
    } else {
      errors.toleranciaPct = '';
    }
  }
};

const validateForm = () => {
  validateField('name');
  validateField('sku');
  validateField('proveedorId');
  validateField('precioBase');
  validateField('margenPct');
  validateField('precioVentaManual');
  validateField('stockInicial');
  validateField('stockMinimo');
  validateField('stockDeseado');
  validateField('toleranciaPct');
  return (
    !errors.name &&
    !errors.sku &&
    !errors.proveedorId &&
    !errors.precioBase &&
    !errors.margenPct &&
    !errors.precioVentaManual &&
    !errors.stockInicial &&
    !errors.stockMinimo &&
    !errors.stockDeseado &&
    !errors.toleranciaPct
  );
};

const loadProducts = async () => {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    if (search.value.trim()) params.set('search', search.value.trim());
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

const handleScan = () => {
  const code = scanInput.value.trim();
  if (!code) return;
  search.value = code;
  loadProducts();
  scanInput.value = '';
};

const searchProveedores = async (term) => {
  proveedorLoading.value = true;
  try {
    const params = new URLSearchParams();
    if (term && term.trim()) params.set('search', term.trim());
    params.set('activo', 'true');
    const { response, data } = await getJson(`/api/v1/proveedores?${params.toString()}`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    proveedoresLookup.value = data || [];
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar proveedores.');
  } finally {
    proveedorLoading.value = false;
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
    form.proveedorId = data.proveedorId || '';
    form.isActive = data.isActive ?? true;
    const base = Number(data.precioBase ?? 0);
    const venta = Number(data.precioVenta ?? base);
    const margen = base > 0 ? ((venta / base) - 1) * 100 : 0;
    form.precioBase = base ? base.toString() : '';
    form.margenPct = Number.isFinite(margen) ? margen.toFixed(2) : '0';
    form.precioManual = false;
    form.precioVentaManual = venta ? venta.toString() : '';
    form.codigoPrincipal = '';
    form.stockInicial = '';
    codes.value = data.codes || [];

    const configResp = await getJson(`/api/v1/productos/${product.id}/stock-config`);
    if (configResp.response.ok) {
      stockConfig.stockMinimo = configResp.data.stockMinimo?.toString?.() ?? '0';
      stockConfig.stockDeseado = configResp.data.stockDeseado?.toString?.() ?? '0';
      stockConfig.toleranciaPct = configResp.data.toleranciaPct?.toString?.() ?? '25';
    } else {
      stockConfig.stockMinimo = '0';
      stockConfig.stockDeseado = '0';
      stockConfig.toleranciaPct = '25';
    }

    clearErrors();
  } catch (err) {
    flash('error', err?.message || 'No se pudo cargar el producto.');
  }
};

const resetForm = () => {
  form.id = '';
  form.name = '';
  form.sku = '';
  form.proveedorId = '';
  form.precioBase = '';
  form.margenPct = '30';
  form.precioManual = false;
  form.precioVentaManual = '';
  form.codigoPrincipal = '';
  form.stockInicial = '';
  form.isActive = true;
  codes.value = [];
  stockConfig.stockMinimo = '0';
  stockConfig.stockDeseado = '0';
  stockConfig.toleranciaPct = '25';
  clearErrors();
};

const saveStockConfig = async (productId) => {
  const payload = {
    stockMinimo: stockConfig.stockMinimo === '' ? 0 : Number(stockConfig.stockMinimo),
    stockDeseado: stockConfig.stockDeseado === '' ? 0 : Number(stockConfig.stockDeseado),
    toleranciaPct: stockConfig.toleranciaPct === '' ? 25 : Number(stockConfig.toleranciaPct)
  };

  const { response, data } = await requestJson(`/api/v1/productos/${productId}/stock-config`, {
    method: 'PATCH',
    body: JSON.stringify(payload)
  });
  if (!response.ok) {
    throw new Error(extractProblemMessage(data));
  }
};

const maybeAddCode = async (productId) => {
  const code = form.codigoPrincipal.trim();
  if (!code) return;
  const exists = codes.value.some((c) => c.code === code);
  if (exists) return;
  const payload = { code };
  const { response, data } = await postJson(`/api/v1/productos/${productId}/codigos`, payload);
  if (!response.ok) {
    throw new Error(extractProblemMessage(data));
  }
  codes.value = [...codes.value, data];
  form.codigoPrincipal = '';
};

const applyStockInicial = async (productId) => {
  if (form.stockInicial === '') return;
  const qty = Number(form.stockInicial);
  if (Number.isNaN(qty) || qty <= 0) return;

  const payload = {
    tipo: 'AJUSTE',
    motivo: 'Stock inicial',
    items: [
      {
        productoId: productId,
        cantidad: qty,
        esIngreso: true
      }
    ]
  };

  const { response, data } = await postJson('/api/v1/stock/ajustes', payload);
  if (!response.ok) {
    throw new Error(extractProblemMessage(data));
  }
  form.stockInicial = '';
};

const saveProduct = async () => {
  if (saving.value) return;
  if (!validateForm()) return;

  saving.value = true;
  try {
    const precioBase = form.precioBase === '' ? null : Number(form.precioBase);
    const precioVenta = form.precioManual
      ? (form.precioVentaManual === '' ? null : Number(form.precioVentaManual))
      : (precioBase == null ? null : Number(precioVentaCalculado.value));

    if (!form.id) {
      const payload = {
        name: form.name.trim(),
        sku: form.sku.trim(),
        proveedorId: form.proveedorId,
        isActive: form.isActive,
        precioBase,
        precioVenta
      };

      const { response, data } = await postJson('/api/v1/productos', payload);
      if (!response.ok) {
        throw new Error(extractProblemMessage(data));
      }
      form.id = data.id;
      codes.value = data.codes || [];
      await saveStockConfig(form.id);
      await applyStockInicial(form.id);
      await maybeAddCode(form.id);
      flash('success', 'Producto creado');
      await loadProducts();
      return;
    }

    const payload = {
      name: form.name.trim() || null,
      sku: form.sku.trim() || null,
      proveedorId: form.proveedorId,
      isActive: form.isActive,
      precioBase,
      precioVenta
    };

    const { response, data } = await requestJson(`/api/v1/productos/${form.id}`, {
      method: 'PATCH',
      body: JSON.stringify(payload)
    });
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }

    codes.value = data.codes || [];
    await saveStockConfig(form.id);
    await maybeAddCode(form.id);
    flash('success', 'Producto actualizado');
    await loadProducts();
  } catch (err) {
    flash('error', err?.message || 'No se pudo guardar el producto.');
  } finally {
    saving.value = false;
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

const validateProveedorField = (field) => {
  if (field === 'name') {
    proveedorErrors.name = proveedorForm.name.trim() ? '' : 'El nombre es obligatorio.';
  }
  if (field === 'telefono') {
    proveedorErrors.telefono = proveedorForm.telefono.trim() ? '' : 'El telefono es obligatorio.';
  }
};

const resetProveedorForm = () => {
  proveedorForm.id = '';
  proveedorForm.name = '';
  proveedorForm.telefono = '';
  proveedorForm.cuit = '';
  proveedorForm.direccion = '';
  proveedorForm.isActive = true;
  proveedorErrors.name = '';
  proveedorErrors.telefono = '';
};

const loadProveedores = async () => {
  proveedorLoadingTable.value = true;
  try {
    const params = new URLSearchParams();
    if (proveedorSearch.value.trim()) params.set('search', proveedorSearch.value.trim());
    if (proveedorActivoFilter.value !== 'all') params.set('activo', proveedorActivoFilter.value);
    const { response, data } = await getJson(`/api/v1/proveedores?${params.toString()}`);
    if (!response.ok) {
      throw new Error(extractProblemMessage(data));
    }
    proveedoresTable.value = data || [];
  } catch (err) {
    flash('error', err?.message || 'No se pudieron cargar proveedores.');
  } finally {
    proveedorLoadingTable.value = false;
  }
};

const selectProveedor = (event, row) => {
  const proveedor = row?.item?.raw ?? row?.item ?? row;
  if (!proveedor?.id) return;
  proveedorForm.id = proveedor.id;
  proveedorForm.name = proveedor.name || '';
  proveedorForm.telefono = proveedor.telefono || '';
  proveedorForm.cuit = proveedor.cuit || '';
  proveedorForm.direccion = proveedor.direccion || '';
  proveedorForm.isActive = proveedor.isActive ?? true;
  proveedorErrors.name = '';
  proveedorErrors.telefono = '';
};

const saveProveedor = async () => {
  if (proveedorSaving.value) return;
  validateProveedorField('name');
  validateProveedorField('telefono');
  if (proveedorErrors.name || proveedorErrors.telefono) return;

  proveedorSaving.value = true;
  try {
    const payload = {
      name: proveedorForm.name.trim(),
      telefono: proveedorForm.telefono.trim(),
      cuit: proveedorForm.cuit.trim() || null,
      direccion: proveedorForm.direccion.trim() || null,
      isActive: proveedorForm.isActive
    };

    if (!proveedorForm.id) {
      const { response, data } = await postJson('/api/v1/proveedores', payload);
      if (!response.ok) {
        throw new Error(extractProblemMessage(data));
      }
      proveedorForm.id = data.id;
      flash('success', 'Proveedor creado');
    } else {
      const { response, data } = await requestJson(`/api/v1/proveedores/${proveedorForm.id}`, {
        method: 'PATCH',
        body: JSON.stringify(payload)
      });
      if (!response.ok) {
        throw new Error(extractProblemMessage(data));
      }
      flash('success', 'Proveedor actualizado');
    }

    await loadProveedores();
    await searchProveedores('');
  } catch (err) {
    flash('error', err?.message || 'No se pudo guardar el proveedor.');
  } finally {
    proveedorSaving.value = false;
  }
};

const syncTabFromRoute = (value) => {
  tab.value = value === 'proveedores' ? 'proveedores' : 'productos';
};

watch(
  () => tab.value,
  (value) => {
    const nextQuery = { ...route.query, tab: value };
    router.replace({ path: '/productos', query: nextQuery });
  }
);

watch(
  () => route.query.tab,
  (value) => {
    syncTabFromRoute(value);
  }
);

watch(
  () => form.precioManual,
  (value) => {
    if (value && !form.precioVentaManual) {
      form.precioVentaManual = precioVentaCalculado.value.toFixed(2);
    }
    validateField('margenPct');
    validateField('precioVentaManual');
  }
);

onMounted(() => {
  syncTabFromRoute(route.query.tab);
  loadProducts();
  loadProveedores();
  searchProveedores('');
});
</script>

<style scoped>
.productos-page {
  animation: fade-in 0.3s ease;
}
</style>
