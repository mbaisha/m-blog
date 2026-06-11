<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FriendListItem } from '@/types/friend'
import { getFriendListApi, createFriendApi, updateFriendApi, deleteFriendApi } from '@/api/friend'

const loading = ref(false)
const friends = ref<FriendListItem[]>([])

/** 对话框 */
const dialogVisible = ref(false)
const dialogTitle = ref('')
const saving = ref(false)
const currentId = ref<string | null>(null)
const form = ref({
  name: '',
  url: '',
  description: '',
  sortOrder: 0,
  isVisible: true
})
const formRef = ref()
const rules = {
  name: [{ required: true, message: '请输入站点名称', trigger: 'blur' }],
  url: [
    { required: true, message: '请输入站点地址', trigger: 'blur' },
    { type: 'url', message: '请输入正确的 URL 格式', trigger: 'blur' }
  ]
}

/** 加载列表 */
async function loadFriends() {
  loading.value = true
  try {
    const res = await getFriendListApi()
    friends.value = (res.data.data || []).sort((a, b) => a.sortOrder - b.sortOrder)
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

/** 打开新建 */
function handleCreate() {
  currentId.value = null
  dialogTitle.value = '添加友情链接'
  form.value = { name: '', url: '', description: '', sortOrder: friends.value.length, isVisible: true }
  dialogVisible.value = true
}

/** 打开编辑 */
function handleEdit(friend: FriendListItem) {
  currentId.value = friend.id
  dialogTitle.value = '编辑友情链接'
  form.value = {
    name: friend.name,
    url: friend.url,
    description: friend.description || '',
    sortOrder: friend.sortOrder,
    isVisible: friend.isVisible
  }
  dialogVisible.value = true
}

/** 保存 */
async function handleSave() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    const data = {
      name: form.value.name,
      url: form.value.url,
      description: form.value.description || null,
      sortOrder: form.value.sortOrder,
      isVisible: form.value.isVisible
    }

    if (currentId.value) {
      await updateFriendApi(currentId.value, data)
      ElMessage.success('友情链接已更新')
    } else {
      await createFriendApi(data)
      ElMessage.success('友情链接已添加')
    }
    dialogVisible.value = false
    await loadFriends()
  } catch {
    // ignore
  } finally {
    saving.value = false
  }
}

/** 删除 */
async function handleDelete(id: string, name: string) {
  try {
    await ElMessageBox.confirm(`确定要删除友情链接「${name}」？`, '确认删除')
    await deleteFriendApi(id)
    ElMessage.success('友情链接已删除')
    await loadFriends()
  } catch {
    // ignore
  }
}

onMounted(() => {
  loadFriends()
})
</script>

<template>
  <div class="friends-page">
    <div class="page-header">
      <h3>友情链接</h3>
      <el-button type="primary" @click="handleCreate">添加链接</el-button>
    </div>

    <el-card shadow="never">
      <el-table :data="friends" v-loading="loading" stripe style="width: 100%">
        <el-table-column label="排序" width="60">
          <template #default="{ row }">{{ row.sortOrder }}</template>
        </el-table-column>
        <el-table-column label="站点名称" min-width="150">
          <template #default="{ row }">{{ row.name }}</template>
        </el-table-column>
        <el-table-column label="站点地址" min-width="250">
          <template #default="{ row }">
            <el-link :href="row.url" target="_blank" type="primary">{{ row.url }}</el-link>
          </template>
        </el-table-column>
        <el-table-column label="描述" min-width="200">
          <template #default="{ row }">{{ row.description || '-' }}</template>
        </el-table-column>
        <el-table-column label="可见" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isVisible ? 'success' : 'info'" size="small">
              {{ row.isVisible ? '可见' : '隐藏' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="170">
          <template #default="{ row }">{{ new Date(row.createdAt).toLocaleDateString('zh-CN') }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">编辑</el-button>
            <el-button link type="danger" @click="handleDelete(row.id, row.name)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 对话框 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="550px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="站点名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入站点名称" maxlength="255" />
        </el-form-item>
        <el-form-item label="站点地址" prop="url">
          <el-input v-model="form.url" placeholder="https://example.com" />
        </el-form-item>
        <el-form-item label="站点描述">
          <el-input v-model="form.description" placeholder="简短描述" maxlength="512" show-word-limit />
        </el-form-item>
        <el-form-item label="排序序号">
          <el-input-number v-model="form.sortOrder" :min="0" />
        </el-form-item>
        <el-form-item label="是否可见">
          <el-switch v-model="form.isVisible" />
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
.friends-page {
  padding: 0;
}
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.page-header h3 {
  margin: 0;
  font-size: 20px;
}
</style>