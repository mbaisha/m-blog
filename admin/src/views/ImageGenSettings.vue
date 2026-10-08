<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getImageGenConfigApi, saveImageGenConfigApi, type ImageGenConfig } from '@/api/imageGen'

const loading = ref(false)
const saving = ref(false)

// 页面内置的默认尺寸预设（首次加载且后端未配置预设时使用）
const DEFAULT_SIZES = [
  '1664x2496', '2496x1664', '1760x2368', '2368x1760',
  '1824x2272', '2272x1824', '2048x2048', '2752x1536',
  '1536x2752', '3072x1376', '1344x3136'
]

const form = ref<ImageGenConfig>({
  baseUrl: '',
  apiKey: '',
  model: '',
  availableModels: '[]',
  defaultSize: '2048x2048',
  availableSizes: '[]',
  defaultQuality: 'standard',
  defaultN: 1,
  timeoutSeconds: 300
})

const availableModelsStr = ref('')
// 尺寸预设列表（可增删）
const availableSizesArr = ref<string[]>([])
// 自定义宽高输入
const customWidth = ref<number | undefined>(undefined)
const customHeight = ref<number | undefined>(undefined)
// 添加预设的输入
const newPresetWidth = ref<number | undefined>(undefined)
const newPresetHeight = ref<number | undefined>(undefined)

async function load() {
  loading.value = true
  try {
    const res = await getImageGenConfigApi()
    if (res.data?.code === 200 && res.data.data) {
      const d = res.data.data
      form.value = {
        baseUrl: d.baseUrl ?? '',
        apiKey: d.apiKey ?? '',
        model: d.model ?? '',
        availableModels: d.availableModels ?? '[]',
        defaultSize: d.defaultSize ?? '2048x2048',
        availableSizes: d.availableSizes ?? '[]',
        defaultQuality: d.defaultQuality ?? 'standard',
        defaultN: d.defaultN ?? 1,
        timeoutSeconds: d.timeoutSeconds ?? 300
      }
      try {
        const arr = JSON.parse(d.availableModels || '[]')
        availableModelsStr.value = Array.isArray(arr) ? arr.join('\n') : ''
      } catch {
        availableModelsStr.value = ''
      }
      try {
        const sizes = JSON.parse(d.availableSizes || '[]')
        availableSizesArr.value = Array.isArray(sizes) && sizes.length > 0
          ? sizes
          : [...DEFAULT_SIZES]
      } catch {
        availableSizesArr.value = [...DEFAULT_SIZES]
      }
      // 若默认尺寸不在预设列表中，加入预设列表（避免下拉看不到当前值）
      if (form.value.defaultSize && !availableSizesArr.value.includes(form.value.defaultSize)) {
        availableSizesArr.value.unshift(form.value.defaultSize)
      }
    }
  } finally {
    loading.value = false
  }
}

/** 应用自定义宽高到默认尺寸 */
function applyCustomSize() {
  const w = customWidth.value
  const h = customHeight.value
  if (!w || !h || w <= 0 || h <= 0) {
    ElMessage.warning('请输入有效的宽度和高度')
    return
  }
  form.value.defaultSize = `${w}x${h}`
  ElMessage.success(`已设为默认尺寸 ${w}x${h}`)
}

/** 添加自定义宽高到预设列表 */
function addPreset() {
  const w = newPresetWidth.value
  const h = newPresetHeight.value
  if (!w || !h || w <= 0 || h <= 0) {
    ElMessage.warning('请输入有效的宽度和高度')
    return
  }
  const size = `${w}x${h}`
  if (availableSizesArr.value.includes(size)) {
    ElMessage.warning('该尺寸已存在')
    return
  }
  availableSizesArr.value.push(size)
  newPresetWidth.value = undefined
  newPresetHeight.value = undefined
  ElMessage.success(`已添加预设 ${size}`)
}

/** 删除预设 */
function removePreset(size: string) {
  availableSizesArr.value = availableSizesArr.value.filter(s => s !== size)
  if (form.value.defaultSize === size) {
    form.value.defaultSize = availableSizesArr.value[0] ?? ''
  }
}

async function save() {
  saving.value = true
  try {
    const models = availableModelsStr.value
      .split('\n')
      .map(s => s.trim())
      .filter(s => s.length > 0)

    if (!form.value.defaultSize) {
      ElMessage.warning('请选择或设置默认尺寸')
      saving.value = false
      return
    }
    // 确保默认尺寸在预设列表中
    if (!availableSizesArr.value.includes(form.value.defaultSize)) {
      availableSizesArr.value.unshift(form.value.defaultSize)
    }

    await saveImageGenConfigApi({
      ...form.value,
      availableModels: JSON.stringify(models),
      availableSizes: JSON.stringify(availableSizesArr.value)
    })
    ElMessage.success('文生图配置已保存')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="image-gen-settings" v-loading="loading">
    <h2>文生图模型设置</h2>
    <p class="desc">配置文生图 API 连接参数，接口路径为 /images/generations。</p>

    <el-form :model="form" label-width="120px" class="settings-form">
      <el-form-item label="Base URL" required>
        <el-input v-model="form.baseUrl" placeholder="https://api.openai.com/v1" />
        <div class="tip">不以 /v1 结尾的地址会自动补全。</div>
      </el-form-item>

      <el-form-item label="API Key" required>
        <el-input v-model="form.apiKey" type="password" show-password placeholder="sk-..." />
      </el-form-item>

      <el-form-item label="默认模型" required>
        <el-input v-model="form.model" placeholder="dall-e-3" />
      </el-form-item>

      <el-form-item label="可用模型列表">
        <el-input v-model="availableModelsStr" type="textarea" :rows="4" placeholder="每行一个模型名称" />
        <div class="tip">每行输入一个模型名称。</div>
      </el-form-item>

      <el-divider content-position="left">默认参数</el-divider>

      <el-form-item label="默认尺寸">
        <div class="size-block">
          <el-select v-model="form.defaultSize" filterable allow-create default-first-option
            placeholder="请选择" class="size-select">
            <el-option v-for="size in availableSizesArr" :key="size" :label="size" :value="size" />
          </el-select>
          <div class="size-row">
            <span class="size-divider">或自定义宽高：</span>
            <el-input-number v-model="customWidth" :min="1" :max="8192" :step="64" controls-position="right" placeholder="宽" class="num-input" />
            <span class="x">×</span>
            <el-input-number v-model="customHeight" :min="1" :max="8192" :step="64" controls-position="right" placeholder="高" class="num-input" />
            <el-button type="primary" plain @click="applyCustomSize">设为默认</el-button>
          </div>
        </div>
        <div class="tip">可从下拉选择预设，或直接输入宽高后点击“设为默认”。</div>
      </el-form-item>

      <el-form-item label="尺寸预设管理">
        <div class="preset-list">
          <el-tag v-for="size in availableSizesArr" :key="size" closable type="info"
            class="preset-tag" @close="removePreset(size)">{{ size }}</el-tag>
        </div>
        <div class="size-row preset-add">
          <el-input-number v-model="newPresetWidth" :min="1" :max="8192" :step="64" controls-position="right" placeholder="宽" class="num-input" />
          <span class="x">×</span>
          <el-input-number v-model="newPresetHeight" :min="1" :max="8192" :step="64" controls-position="right" placeholder="高" class="num-input" />
          <el-button type="success" plain @click="addPreset">添加预设</el-button>
        </div>
        <div class="tip">点击标签的 × 可删除预设；输入宽高后点击“添加预设”可加入下拉列表。</div>
      </el-form-item>

      <el-form-item label="默认质量">
        <el-select v-model="form.defaultQuality" placeholder="请选择">
          <el-option label="不发送（兼容模式）" value="" />
          <el-option label="标准 (standard)" value="standard" />
          <el-option label="高清 (hd)" value="hd" />
        </el-select>
        <div class="tip">选「不发送」可兼容不支持 quality 参数的供应商。</div>
      </el-form-item>

      <el-form-item label="默认数量">
        <el-input-number v-model="form.defaultN" :min="1" :max="10" />
      </el-form-item>

      <el-form-item label="请求超时">
        <el-input-number v-model="form.timeoutSeconds" :min="0" :max="3600" :step="10" controls-position="right" />
        <span class="unit">秒</span>
        <div class="tip">请求文生图 API 的超时时间，0 表示不限制（默认 300 秒）。</div>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="save" :loading="saving">保存配置</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped>
.image-gen-settings { padding: 20px; max-width: 760px; }
h2 { margin: 0 0 8px 0; font-size: 20px; }
.desc { color: #909399; margin-bottom: 24px; }
.settings-form { max-width: 640px; }
.tip { color: #909399; font-size: 12px; margin-top: 4px; }
.unit { margin-left: 8px; color: #666; }

.size-block { display: flex; flex-direction: column; gap: 10px; }
.size-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.size-select { width: 220px; }
.num-input { width: 130px; }
.size-divider { color: #909399; font-size: 13px; margin-right: 4px; }
.x { color: #909399; }

.preset-list { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 12px; }
.preset-tag { min-width: 96px; justify-content: center; }
.preset-add { margin-top: 4px; }
</style>
