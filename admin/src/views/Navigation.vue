<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { NavigationItemNode, CreateNavigationItemRequest } from '@/types/pages'
import { getNavigationListApi, getNavigationItemApi, createNavigationItemApi, updateNavigationItemApi, deleteNavigationItemApi, batchUpdateSortApi } from '@/api/navigation'
import Sortable from 'sortablejs'
import IconPicker from '@/components/IconPicker.vue'

interface FlatNode {
  id: string
  title: string
  url: string
  location: string
  sortOrder: number
  isVisible: boolean
  parentId: string | null
  depth: number
}

const loading = ref(false)
const saving = ref(false)
const treeData = ref<NavigationItemNode[]>([])
const flatData = ref<FlatNode[]>([])
const expandedKeys = ref<Set<string>>(new Set())
const dialogVisible = ref(false)
const dialogTitle = ref('')
const currentId = ref<string | null>(null)
const tableRef = ref()
let sortableInstance: Sortable | null = null

const form = ref<CreateNavigationItemRequest>({
  title: '',
  url: '',
  parentId: null,
  icon: '',
  openInNewTab: false,
  sortOrder: 0,
  isVisible: true,
  location: 'header'
})
const formRef = ref()
const rules = {
  title: [{ required: true, message: '请输入菜单名称', trigger: 'blur' }],
  url: [{ required: true, message: '请输入链接 URL', trigger: 'blur' }]
}

const flatItems = computed(() => {
  const result: { id: string; title: string; depth: number }[] = []
  function walk(items: NavigationItemNode[], depth: number) {
    for (const item of items) {
      result.push({ id: item.id, title: item.title, depth })
      walk(item.children, depth + 1)
    }
  }
  walk(treeData.value, 0)
  return result
})

function flattenTree(nodes: NavigationItemNode[], depth = 0): FlatNode[] {
  const result: FlatNode[] = []
  for (const n of nodes) {
    result.push({
      id: n.id,
      title: n.title,
      url: n.url,
      location: n.location,
      sortOrder: n.sortOrder,
      isVisible: n.isVisible,
      parentId: n.parentId,
      depth,
    })
    result.push(...flattenTree(n.children, depth + 1))
  }
  return result
}

function syncFlat() {
  flatData.value = flattenTree(treeData.value)
}

// 树形展开/折叠
const visibleData = computed(() => {
  return flatData.value.filter(n => n.depth === 0 || expandedKeys.value.has(n.parentId!))
})

function hasChildren(id: string): boolean {
  return flatData.value.some(n => n.parentId === id)
}

function toggleExpand(id: string) {
  const next = new Set(expandedKeys.value)
  if (next.has(id)) {
    next.delete(id)
  } else {
    next.add(id)
  }
  expandedKeys.value = next
}

async function loadTree() {
  loading.value = true
  try {
    const res = await getNavigationListApi()
    treeData.value = res.data.data || []
    syncFlat()
    await nextTick()
    initSortable()
  } finally {
    loading.value = false
  }
}

function initSortable() {
  if (sortableInstance) {
    sortableInstance.destroy()
    sortableInstance = null
  }
  if (!tableRef.value) return
  const el = tableRef.value.$el as HTMLElement
  const tbody = el.querySelector('tbody')
  if (!tbody) return

  sortableInstance = new Sortable(tbody, {
    handle: '.drag-handle',
    animation: 150,
    onEnd: handleSortEnd
  })
}

async function handleSortEnd() {
  const rows = tableRef.value.$el.querySelectorAll('tbody tr')
  const sortItems: { id: string; sortOrder: number; parentId: string | null }[] = []
  rows.forEach((row: HTMLElement, index: number) => {
    const rowKey = row.getAttribute('row-key')
    if (!rowKey) return
    const node = flatData.value.find(n => n.id === rowKey)
    if (node) {
      sortItems.push({ id: node.id, sortOrder: index, parentId: node.parentId })
    }
  })

  saving.value = true
  try {
    await batchUpdateSortApi(sortItems)
    ElMessage.success('排序已更新')
    await loadTree()
  } finally {
    saving.value = false
  }
}

function handleCreate(parentId: string | null = null) {
  currentId.value = null
  dialogTitle.value = parentId ? '新建子菜单' : '新建菜单项'
  form.value = { title: '', url: '', parentId, icon: '', openInNewTab: false, sortOrder: 0, isVisible: true, location: 'header' }
  dialogVisible.value = true
}

async function handleEdit(id: string) {
  currentId.value = id
  dialogTitle.value = '编辑菜单项'
  try {
    const res = await getNavigationItemApi(id)
    const d = res.data.data
    if (d) {
      form.value = {
        title: d.title,
        url: d.url,
        parentId: d.parentId,
        icon: d.icon || '',
        openInNewTab: d.openInNewTab,
        sortOrder: d.sortOrder,
        isVisible: d.isVisible,
        location: d.location
      }
    }
  } catch {
    ElMessage.error('加载菜单项失败')
    return
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    if (currentId.value) {
      await updateNavigationItemApi(currentId.value, form.value)
      ElMessage.success('菜单项更新成功')
    } else {
      await createNavigationItemApi(form.value)
      ElMessage.success('菜单项创建成功')
    }
    dialogVisible.value = false
    await loadTree()
  } finally {
    saving.value = false
  }
}

async function handleDelete(id: string, title: string) {
  try {
    await ElMessageBox.confirm(`确定要删除菜单「${title}」及其子菜单？`, '确认删除')
    await deleteNavigationItemApi(id)
    ElMessage.success('菜单项已删除')
    await loadTree()
  } catch { /* ignore */ }
}

onMounted(loadTree)
</script>

<template>
  <div class="nav-page">
    <div class="page-header">
      <h3>导航菜单管理</h3>
      <el-button type="primary" @click="handleCreate(null)">新建菜单项</el-button>
    </div>

    <el-card shadow="never">
      <el-table ref="tableRef" :data="visibleData" v-loading="loading" stripe row-key="id" style="width:100%">
        <el-table-column label="排序" width="60">
          <template #default="{ row }">
            <span class="drag-handle" title="拖拽排序">⠿</span>
          </template>
        </el-table-column>
        <el-table-column label="菜单名称" min-width="220">
          <template #default="{ row }">
            <div class="tree-cell" :style="{ paddingLeft: row.depth * 24 + 'px' }">
              <button
                v-if="hasChildren(row.id)"
                class="expand-btn"
                :class="{ expanded: expandedKeys.has(row.id) }"
                @click="toggleExpand(row.id)"
              >
                <svg width="10" height="10" viewBox="0 0 10 10" fill="currentColor">
                  <path d="M3 1l5 4-5 4" />
                </svg>
              </button>
              <span v-else class="expand-placeholder" />
              <span>{{ row.title }}</span>
              <el-tag v-if="row.location !== 'header'" size="small" type="info" style="margin-left:6px">
                {{ ({ header: '顶部', footer: '底部', both: '全部' } as Record<string, string>)[row.location] || row.location }}
              </el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="链接" min-width="200">
          <template #default="{ row }"><code>{{ row.url }}</code></template>
        </el-table-column>
        <el-table-column label="位置" width="90">
          <template #default="{ row }">
            <span>{{ ({ header: '顶部', footer: '底部', both: '全部' } as Record<string, string>)[row.location] || row.location }}</span>
          </template>
        </el-table-column>
        <el-table-column label="可见" width="70">
          <template #default="{ row }">
            <el-tag :type="row.isVisible ? 'success' : 'danger'" size="small">{{ row.isVisible ? '是' : '否' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleCreate(row.id)">加子项</el-button>
            <el-button link type="primary" @click="handleEdit(row.id)">编辑</el-button>
            <el-button link type="danger" @click="handleDelete(row.id, row.title)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="550px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="菜单名称" prop="title">
          <el-input v-model="form.title" placeholder="例如：关于我们" maxlength="50" />
        </el-form-item>
        <el-form-item label="链接 URL" prop="url">
          <el-input v-model="form.url" placeholder="例如：/about 或 https://example.com" />
        </el-form-item>
        <el-form-item label="父级菜单">
          <el-select v-model="form.parentId" clearable placeholder="无（顶级菜单）" style="width:100%">
            <el-option v-for="item in flatItems" :key="item.id" :label="'--'.repeat(item.depth) + ' ' + item.title" :value="item.id" :disabled="item.id === currentId" />
          </el-select>
        </el-form-item>
        <el-form-item label="图标">
          <IconPicker v-model="form.icon" mode="icons" placeholder="选择或输入图标名称" />
        </el-form-item>
        <div class="form-row">
          <el-form-item label="显示位置">
            <el-select v-model="form.location" style="width:120px">
              <el-option label="顶部导航" value="header" />
              <el-option label="底部导航" value="footer" />
              <el-option label="全部" value="both" />
            </el-select>
          </el-form-item>
          <el-form-item label="新窗口打开" style="margin-left:24px">
            <el-switch v-model="form.openInNewTab" />
          </el-form-item>
          <el-form-item label="对外可见">
            <el-switch v-model="form.isVisible" />
          </el-form-item>
        </div>
        <el-form-item label="排序">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.nav-page { padding: 0; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.page-header h3 { margin: 0; font-size: 20px; }
.drag-handle {
  cursor: grab;
  user-select: none;
  font-size: 18px;
  color: #909399;
  display: inline-block;
  padding: 4px;
}
.drag-handle:active {
  cursor: grabbing;
}
.form-row {
  display: flex;
  align-items: center;
}
.tree-cell {
  display: flex;
  align-items: center;
  gap: 4px;
}
.expand-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 18px;
  height: 18px;
  border: 1px solid #d9d9d9;
  border-radius: 3px;
  background: #fff;
  cursor: pointer;
  color: #606266;
  flex-shrink: 0;
  transition: transform 0.2s;
  padding: 0;
  line-height: 1;
}
.expand-btn:hover {
  border-color: #409eff;
  color: #409eff;
}
.expand-btn.expanded svg {
  transform: rotate(90deg);
}
.expand-btn svg {
  transition: transform 0.2s;
}
.expand-placeholder {
  width: 18px;
  flex-shrink: 0;
}
</style>