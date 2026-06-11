<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'
import { getDashboardStatsApi, type DashboardStats } from '@/api/dashboard'
import { getVisitStatsApi } from '@/api/visit'

const stats = ref<DashboardStats>({
  articleCount: 0,
  draftCount: 0,
  pendingCommentCount: 0,
  todayViews: 0,
  totalViews: 0,
  mediaCount: 0,
  categoryCount: 0,
  tagCount: 0,
})

const trendChartRef = ref<HTMLElement>()
const hotChartRef = ref<HTMLElement>()

const statCards = [
  { label: '文章总数', key: 'articleCount' as const, icon: 'Document', color: '#409eff' },
  { label: '草稿数量', key: 'draftCount' as const, icon: 'Edit', color: '#e6a23c' },
  { label: '待审核评论', key: 'pendingCommentCount' as const, icon: 'ChatDotSquare', color: '#f56c6c' },
  { label: '今日访问', key: 'todayViews' as const, icon: 'View', color: '#67c23a' },
  { label: '总访问量', key: 'totalViews' as const, icon: 'DataLine', color: '#909399' },
  { label: '媒体数量', key: 'mediaCount' as const, icon: 'Picture', color: '#409eff' },
]

async function loadStats() {
  try {
    const [statsRes, visitRes] = await Promise.all([
      getDashboardStatsApi(),
      getVisitStatsApi().catch(() => null),
    ])
    if (statsRes.data.code === 200 && statsRes.data.data) {
      stats.value = statsRes.data.data
    }
    // Render charts if visit data available
    const visitData = visitRes?.data?.data
    if (visitData) {
      nextTick(() => {
        renderTrendChart(visitData.dailyTrend || [])
        renderHotPages(visitData.popularPages || [])
      })
    }
  } catch {
    ElMessage.error('加载统计数据失败')
  }
}

function renderTrendChart(trend: { date: string; views: number; visitors: number }[]) {
  if (!trendChartRef.value || trend.length === 0) return
  const chart = echarts.init(trendChartRef.value)
  chart.setOption({
    tooltip: { trigger: 'axis' },
    legend: { data: ['访问量', '访客数'] },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'category', data: trend.map(t => t.date.slice(5)), boundaryGap: false },
    yAxis: { type: 'value', minInterval: 1 },
    series: [
      { name: '访问量', type: 'line', smooth: true, data: trend.map(t => t.views), itemStyle: { color: '#409eff' }, lineStyle: { width: 2 } },
      { name: '访客数', type: 'line', smooth: true, data: trend.map(t => t.visitors), itemStyle: { color: '#67c23a' }, lineStyle: { width: 2 } },
    ],
  })
  window.addEventListener('resize', () => chart.resize())
}

function renderHotPages(pages: { pagePath: string; count: number }[]) {
  if (!hotChartRef.value || pages.length === 0) return
  const chart = echarts.init(hotChartRef.value)
  chart.setOption({
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'value', minInterval: 1 },
    yAxis: { type: 'category', data: pages.slice(0, 5).map(p => p.pagePath).reverse(), axisLabel: { fontSize: 11 } },
    series: [{ type: 'bar', data: pages.slice(0, 5).map(p => p.count).reverse(), itemStyle: { color: '#f56c6c' } }],
  })
  window.addEventListener('resize', () => chart.resize())
}

onMounted(loadStats)
</script>

<template>
  <div class="dashboard">
    <h3 class="page-title">仪表盘</h3>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="stats-row">
      <el-col v-for="stat in statCards" :key="stat.label" :xs="12" :sm="8" :md="6" :lg="4">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-inner" :style="{ borderLeftColor: stat.color }">
            <div class="stat-value">{{ stats[stat.key] }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 图表区域 -->
    <el-row :gutter="16" class="chart-row">
      <el-col :span="14">
        <el-card shadow="hover">
          <template #header>近 30 天访问趋势</template>
          <div ref="trendChartRef" style="width: 100%; height: 260px"></div>
        </el-card>
      </el-col>
      <el-col :span="10">
        <el-card shadow="hover">
          <template #header>热门页面 Top 5</template>
          <div ref="hotChartRef" style="width: 100%; height: 260px"></div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.dashboard {
  padding: 0;
}
.page-title {
  margin: 0 0 16px;
  font-size: 20px;
}
.stats-row {
  margin-bottom: 16px;
}
.stat-card {
  margin-bottom: 16px;
}
.stat-inner {
  padding: 16px;
  border-left: 4px solid;
}
.stat-value {
  font-size: 28px;
  font-weight: bold;
  color: #303133;
}
.stat-label {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}
.chart-row {
  margin-top: 8px;
}
</style>