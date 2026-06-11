<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FooterConfig } from '@/types/pages'
import { getFooterConfigApi, saveFooterConfigApi } from '@/api/footer'
import IconPicker from '@/components/IconPicker.vue'

interface BlockLink {
  label: string
  url: string
}

interface FooterBlock {
  title: string
  links: BlockLink[]
}

interface SocialLink {
  platform: string
  url: string
  icon: string
}

const loading = ref(false)
const saving = ref(false)
const form = ref({
  copyright: '© 2024 My Blog',
  icpNumber: '',
  icpUrl: '',
  policeNumber: '',
  policeUrl: '',
  socialLinks: '[]',
  blocks: '[]',
  style: 'detailed',
  isVisible: true,
  contactEmail: '',
  contactWeChat: '',
  contactPhone: ''
})

const blocks = reactive<FooterBlock[]>([])
const socialLinksList = reactive<SocialLink[]>([])

// --- Block editor ---
function parseBlocks() {
  blocks.length = 0
  try {
    const parsed = JSON.parse(form.value.blocks)
    if (Array.isArray(parsed)) {
      parsed.forEach(b => blocks.push(b))
    }
  } catch { /* ignore */ }
}

function syncBlocksToJson() {
  form.value.blocks = JSON.stringify(blocks)
}

function addBlock() {
  blocks.push({ title: '', links: [] })
  syncBlocksToJson()
}

function removeBlock(index: number) {
  blocks.splice(index, 1)
  syncBlocksToJson()
}

function addBlockLink(blockIndex: number) {
  blocks[blockIndex].links.push({ label: '', url: '' })
  syncBlocksToJson()
}

function removeBlockLink(blockIndex: number, linkIndex: number) {
  blocks[blockIndex].links.splice(linkIndex, 1)
  syncBlocksToJson()
}

function onBlockChange() {
  syncBlocksToJson()
}

// --- Social Links editor ---
function parseSocialLinks() {
  socialLinksList.length = 0
  try {
    const parsed = JSON.parse(form.value.socialLinks)
    if (Array.isArray(parsed)) {
      parsed.forEach(s => socialLinksList.push(s))
    }
  } catch { /* ignore */ }
}

function syncSocialLinksToJson() {
  form.value.socialLinks = JSON.stringify(socialLinksList)
}

function addSocialLink() {
  socialLinksList.push({ platform: '', url: '', icon: '' })
  syncSocialLinksToJson()
}

function removeSocialLink(index: number) {
  socialLinksList.splice(index, 1)
  syncSocialLinksToJson()
}

function onSocialChange() {
  syncSocialLinksToJson()
}

async function loadConfig() {
  loading.value = true
  try {
    const res = await getFooterConfigApi()
    const d = res.data.data
    if (d && d.id) {
      form.value = {
        copyright: d.copyright || '',
        icpNumber: d.icpNumber || '',
        icpUrl: d.icpUrl || '',
        policeNumber: d.policeNumber || '',
        policeUrl: d.policeUrl || '',
        socialLinks: d.socialLinks || '[]',
        blocks: d.blocks || '[]',
        style: d.style || 'detailed',
        isVisible: d.isVisible,
        contactEmail: d.contactEmail || '',
        contactWeChat: d.contactWeChat || '',
        contactPhone: d.contactPhone || ''
      }
      parseBlocks()
      parseSocialLinks()
    }
  } finally {
    loading.value = false
  }
}

async function handleSave() {
  saving.value = true
  try {
    // Sync visual editors to JSON before saving
    syncBlocksToJson()
    syncSocialLinksToJson()
    await saveFooterConfigApi(form.value)
    ElMessage.success('页脚配置已更新')
  } finally {
    saving.value = false
  }
}

onMounted(loadConfig)
</script>

<template>
  <div class="footer-page">
    <div class="page-header">
      <h3>页脚配置</h3>
      <el-button type="primary" :loading="saving" @click="handleSave">保存配置</el-button>
    </div>

    <el-card shadow="never" v-loading="loading">
      <el-form label-width="130px">
        <el-form-item label="版权信息">
          <el-input v-model="form.copyright" placeholder="例如：© 2024 My Blog. All rights reserved." />
        </el-form-item>

        <el-divider content-position="left">联系方式</el-divider>
        <el-form-item label="邮箱">
          <el-input v-model="form.contactEmail" placeholder="contact@example.com" style="max-width: 320px" />
        </el-form-item>
        <el-form-item label="微信">
          <el-input v-model="form.contactWeChat" placeholder="微信号" style="max-width: 320px" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="form.contactPhone" placeholder="+86 138-0000-0000" style="max-width: 320px" />
        </el-form-item>

        <el-divider content-position="left">备案信息</el-divider>
        <el-form-item label="ICP 备案号">
          <el-input v-model="form.icpNumber" placeholder="例如：京ICP备2024000000号" style="max-width: 400px" />
        </el-form-item>
        <el-form-item label="ICP 备案链接">
          <el-input v-model="form.icpUrl" placeholder="https://beian.miit.gov.cn/" />
        </el-form-item>
        <el-form-item label="公安备案号">
          <el-input v-model="form.policeNumber" placeholder="例如：京公网安备 11000000000000号" style="max-width: 400px" />
        </el-form-item>
        <el-form-item label="公安备案链接">
          <el-input v-model="form.policeUrl" placeholder="http://www.beian.gov.cn/" />
        </el-form-item>

        <el-divider content-position="left">页脚样式</el-divider>
        <el-form-item label="页脚样式">
          <el-radio-group v-model="form.style">
            <el-radio value="detailed">详细模式（含区块链接）</el-radio>
            <el-radio value="simple">简洁模式（仅版权）</el-radio>
            <el-radio value="centered">居中模式</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="显示页脚">
          <el-switch v-model="form.isVisible" />
        </el-form-item>

        <el-divider content-position="left">社交媒体链接</el-divider>
        <el-form-item label="社交链接">
          <div class="social-list">
            <div v-for="(s, i) in socialLinksList" :key="i" class="social-item">
              <el-input v-model="s.platform" placeholder="平台" style="width:100px" @input="onSocialChange" />
              <el-input v-model="s.url" placeholder="URL" style="width:280px" @input="onSocialChange" />
              <div style="width:80px">
                <IconPicker v-model="s.icon" mode="platforms" placeholder="平台" @update:model-value="onSocialChange" />
              </div>
              <el-button link type="danger" @click="removeSocialLink(i)">删除</el-button>
            </div>
            <el-button size="small" @click="addSocialLink">+ 添加社交链接</el-button>
          </div>
        </el-form-item>

        <el-divider content-position="left">页脚区块（栏目链接）</el-divider>
        <el-form-item label="区块列表">
          <div class="blocks-list">
            <div v-for="(block, bi) in blocks" :key="bi" class="block-card">
              <div class="block-header">
                <el-input v-model="block.title" placeholder="区块标题（如：关于）" style="width:250px" @input="onBlockChange" />
                <el-button link type="danger" @click="removeBlock(bi)">删除区块</el-button>
              </div>
              <div class="block-links">
                <div v-for="(link, li) in block.links" :key="li" class="block-link-row">
                  <el-input v-model="link.label" placeholder="链接标题" style="width:200px" @input="onBlockChange" />
                  <el-input v-model="link.url" placeholder="链接 URL" style="width:250px" @input="onBlockChange" />
                  <el-button link type="danger" @click="removeBlockLink(bi, li)">删除</el-button>
                </div>
                <el-button size="small" @click="addBlockLink(bi)">+ 添加链接</el-button>
              </div>
            </div>
            <el-button @click="addBlock">+ 添加区块</el-button>
          </div>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.footer-page { padding: 0; }
.page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.page-header h3 { margin: 0; font-size: 20px; }

.social-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}
.social-item {
  display: flex;
  align-items: center;
  gap: 8px;
}
.blocks-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: 100%;
}
.block-card {
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  padding: 12px;
  background: #fafafa;
}
.block-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}
.block-links {
  padding-left: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.block-link-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>