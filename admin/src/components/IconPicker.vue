<template>
  <el-popover
    placement="bottom-start"
    :width="360"
    trigger="click"
    :visible="visible"
    @show="visible = true"
    @hide="visible = false"
  >
    <template #reference>
      <div class="icon-picker-trigger" :class="{ 'is-filled': !!modelValue }">
        <el-input
          :modelValue="modelValue"
          @update:modelValue="handleInput"
          :placeholder="placeholder"
          clearable
          style="width:100%"
        >
          <template #prefix>
            <component :is="iconComponent" v-if="modelValue && mode === 'icons'" class="icon-preview" />
            <span v-else-if="modelValue && mode === 'platforms'" class="platform-preview-text">{{ modelValue }}</span>
          </template>
          <template #suffix>
            <el-button link @click.stop="visible = true">
              <el-icon><Grid /></el-icon>
            </el-button>
          </template>
        </el-input>
      </div>
    </template>

    <div class="icon-picker-popover">
      <el-input
        v-model="searchText"
        placeholder="搜索图标..."
        clearable
        size="small"
        class="search-input"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>

      <!-- 图标模式 -->
      <template v-if="mode === 'icons'">
        <div class="icon-grid">
          <div
            v-for="icon in filteredIcons"
            :key="icon"
            class="icon-item"
            :class="{ selected: modelValue === icon }"
            @click="selectIcon(icon)"
          >
            <el-icon :size="20">
              <component :is="icon" />
            </el-icon>
            <span class="icon-name">{{ icon }}</span>
          </div>
          <div v-if="filteredIcons.length === 0" class="no-results">
            未找到匹配图标
          </div>
        </div>
      </template>

      <!-- 社交平台模式 -->
      <template v-else>
        <div class="platform-list">
          <div
            v-for="p in filteredPlatforms"
            :key="p.value"
            class="platform-item"
            :class="{ selected: modelValue === p.value }"
            @click="selectIcon(p.value)"
          >
            <span class="platform-label">{{ p.label }}</span>
            <span class="platform-value">{{ p.value }}</span>
          </div>
          <div v-if="filteredPlatforms.length === 0" class="no-results">
            未找到匹配平台
          </div>
        </div>
      </template>
    </div>
  </el-popover>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Grid, Search } from '@element-plus/icons-vue'

interface Platform {
  label: string
  value: string
}

const props = withDefaults(defineProps<{
  modelValue: string | null | undefined
  mode?: 'icons' | 'platforms'
  placeholder?: string
}>(), {
  mode: 'icons',
  placeholder: '选择或输入图标',
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | null): void
}>()

const visible = ref(false)
const searchText = ref('')

// 所有可用图标（使用正确的全局组件名）
const allIcons: string[] = [
  'Link', 'HomeFilled', 'Document', 'DocumentAdd', 'Notebook', 'Collection', 'CollectionTag',
  'Files', 'FolderOpened', 'Folder', 'Picture', 'DataLine', 'Odometer', 'TrendCharts',
  'Histogram', 'ChatDotSquare', 'ChatLineSquare', 'Comment', 'ChatDotRound', 'ChatLineRound',
  'User', 'UserFilled', 'Avatar', 'Setting', 'Tools', 'Management', 'Edit', 'EditPen',
  'Delete', 'Plus', 'Search', 'Download', 'Upload', 'Share', 'Star', 'StarFilled',
  'InfoFilled', 'WarningFilled', 'CircleCheck', 'CircleClose', 'CirclePlus',
  'Clock', 'Calendar', 'Bell', 'BellFilled', 'Message', 'MessageBox', 'Flag', 'PriceTag',
  'Medal', 'Trophy', 'Headset', 'Key', 'Lock', 'Unlock', 'Menu', 'Grid', 'List',
  'Refresh', 'FullScreen', 'Expand', 'Fold', 'Rank',
  'Cellphone', 'Iphone', 'Monitor', 'Printer', 'Computer',
  'Camera', 'CameraFilled', 'VideoCamera', 'VideoCameraFilled',
  'Microphone', 'Mic', 'Mute', 'MuteNotification',
  'Location', 'LocationFilled', 'MapLocation', 'Compass',
  'Aim', 'Coordinate', 'Connection',
  'Coffee', 'CoffeeCup', 'ColdDrink', 'HotWater', 'MilkTea',
  'IceCream', 'IceCreamRound', 'IceCreamSquare', 'Dessert',
  'Burger', 'Fries', 'Chicken', 'Food', 'ForkSpoon', 'KnifeFork',
  'Grape', 'Cherry', 'Apple', 'Watermelon', 'Orange', 'Lemon',
  'Baseball', 'Basketball', 'Football',
  'MagicStick', 'Crop', 'Scissors', 'Brush', 'BrushFilled',
  'Money', 'Coin', 'CreditCard', 'Goods', 'GoodsFilled',
  'Home', 'House', 'OfficeBuilding',
  'DArrowLeft', 'DArrowRight', 'ArrowLeft', 'ArrowRight',
  'ArrowUp', 'ArrowDown', 'CaretTop', 'CaretBottom',
  'CaretLeft', 'CaretRight', 'Switch',
  'Sort', 'Filter', 'AddLocation', 'DeleteLocation',
  'CircleCheckFilled', 'CircleCloseFilled', 'CirclePlusFilled',
  'SuccessFilled', 'Warning', 'Remove', 'Minus', 'Loading',
  'CopyDocument', 'DeleteDocument',
  'DocumentCopy', 'DocumentDelete', 'DocumentRemove', 'DocumentChecked',
  'Ticket', 'Tickets', 'DataAnalysis', 'DataBoard',
  'Dish', 'DishDot', 'Goblet', 'GobletFull',
  'GobletSquare', 'GobletSquareFull', 'Bowl', 'Mug',
  'Sunny', 'Cloudy', 'Lightning', 'WindPower',
  'Moon', 'MoonNight',
]

// 社交平台列表
const platforms: Platform[] = [
  { label: 'GitHub', value: 'github' },
  { label: 'Twitter / X', value: 'twitter' },
  { label: '微博', value: 'weibo' },
  { label: '知乎', value: 'zhihu' },
  { label: 'Bilibili', value: 'bilibili' },
  { label: 'LinkedIn', value: 'linkedin' },
  { label: 'Facebook', value: 'facebook' },
  { label: 'YouTube', value: 'youtube' },
  { label: 'RSS', value: 'rss' },
  { label: '邮箱', value: 'email' },
]

const filteredIcons = computed(() => {
  if (!searchText.value) return allIcons
  const q = searchText.value.toLowerCase()
  return allIcons.filter(i => i.toLowerCase().includes(q))
})

const filteredPlatforms = computed(() => {
  if (!searchText.value) return platforms
  const q = searchText.value.toLowerCase()
  return platforms.filter(p =>
    p.label.toLowerCase().includes(q) || p.value.toLowerCase().includes(q)
  )
})

const iconComponent = computed(() => {
  if (!props.modelValue) return null
  // For icon mode, the component is resolved by global name via <component :is="..." />
  // We just return the string name
  return props.modelValue
})

function handleInput(val: string) {
  emit('update:modelValue', val)
}

function selectIcon(name: string) {
  emit('update:modelValue', name)
  visible.value = false
  searchText.value = ''
}
</script>

<style scoped>
.icon-picker-trigger {
  width: 100%;
}

.search-input {
  margin-bottom: 8px;
}

.icon-picker-popover {
  max-height: 320px;
  overflow-y: auto;
}

.icon-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 4px;
}

.icon-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  padding: 6px 2px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
  border: 1px solid transparent;
}

.icon-item:hover {
  background-color: #f0f5ff;
  border-color: #d6e4ff;
}

.icon-item.selected {
  background-color: #e6f7ff;
  border-color: #1890ff;
  color: #1890ff;
}

.icon-name {
  font-size: 10px;
  color: #666;
  text-align: center;
  word-break: break-all;
  line-height: 1.2;
  max-width: 60px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.icon-item.selected .icon-name {
  color: #1890ff;
}

.platform-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.platform-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
  border: 1px solid transparent;
}

.platform-item:hover {
  background-color: #f0f5ff;
  border-color: #d6e4ff;
}

.platform-item.selected {
  background-color: #e6f7ff;
  border-color: #1890ff;
}

.platform-label {
  font-weight: 500;
  font-size: 14px;
}

.platform-value {
  color: #999;
  font-size: 12px;
  font-family: monospace;
}

.platform-item.selected .platform-label {
  color: #1890ff;
}

.platform-item.selected .platform-value {
  color: #1890ff;
}

.icon-preview {
  font-size: 16px;
}

.platform-preview-text {
  font-size: 13px;
  color: #666;
  text-transform: capitalize;
}

.no-results {
  grid-column: 1 / -1;
  text-align: center;
  padding: 20px;
  color: #999;
  font-size: 13px;
}
</style>