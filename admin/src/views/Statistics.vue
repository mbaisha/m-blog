<template>
  <div class="stats-page">
    <div class="page-header">
      <h3>数据统计</h3>
    </div>

    <!-- 概览卡片 -->
    <el-row :gutter="16" class="stat-cards">
      <el-col :span="8">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">今日访问</div>
            <div class="stat-value">{{ stats.todayViews }}</div>
            <div class="stat-sub">访客 {{ stats.todayVisitors }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">昨日访问</div>
            <div class="stat-value">{{ stats.yesterdayViews }}</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="never">
          <div class="stat-item">
            <div class="stat-label">累计</div>
            <div class="stat-value">{{ stats.totalViews }}</div>
            <div class="stat-sub">访客 {{ stats.totalVisitors }}</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-top: 16px">
      <!-- 近 30 天访问趋势 -->
      <el-col :span="16">
        <el-card shadow="never">
          <template #header><span>近 30 天访问趋势</span></template>
          <div ref="trendChartRef" style="width: 100%; height: 300px"></div>
        </el-card>
      </el-col>

      <!-- 设备分布 -->
      <el-col :span="8">
        <el-card shadow="never">
          <template #header><span>设备分布（近 30 天）</span></template>
          <div ref="deviceChartRef" style="width: 100%; height: 300px"></div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-top: 16px">
      <!-- 热门页面 -->
      <el-col :span="12">
        <el-card shadow="never">
          <template #header><span>热门页面 Top 10</span></template>
          <div ref="popularChartRef" style="width: 100%; height: 300px"></div>
        </el-card>
      </el-col>

      <!-- 评论趋势 -->
      <el-col :span="12">
        <el-card shadow="never">
          <template #header><span>评论趋势（近 30 天）</span></template>
          <div ref="commentChartRef" style="width: 100%; height: 300px"></div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, type Ref } from 'vue'
import * as echarts from 'echarts'
import { getVisitStatsApi } from '@/api/visit'
import type { VisitStats, DeviceDistributionItem, CommentTrendItem } from '@/types/visit'

const stats = ref<VisitStats>({
  todayViews: 0,
  todayVisitors: 0,
  yesterdayViews: 0,
  totalViews: 0,
  totalVisitors: 0,
  popularPages: [],
  dailyTrend: [],
  deviceDistribution: [],
  commentTrend: [],
}) as Ref<VisitStats>

const trendChartRef = ref<HTMLElement>()
const popularChartRef = ref<HTMLElement>()
const deviceChartRef = ref<HTMLElement>()
const commentChartRef = ref<HTMLElement>()

async function fetchStats() {
  const res = await getVisitStatsApi()
  if (res.data.code === 200 && res.data.data) {
    stats.value = res.data.data as VisitStats
    nextTick(renderCharts)
  }
}

function renderCharts() {
  renderTrendChart()
  renderPopularChart()
  renderDeviceChart()
  renderCommentChart()
}

function renderTrendChart() {
  if (!trendChartRef.value) return
  const chart = echarts.init(trendChartRef.value)
  const trend = stats.value.dailyTrend
  chart.setOption({
    tooltip: { trigger: 'axis' },
    legend: { data: ['访问量', '访客数'] },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'category', data: trend.map(t => t.date.slice(5)), boundaryGap: false },
    yAxis: { type: 'value', minInterval: 1 },
    series: [
      { name: '访问量', type: 'line', smooth: true, data: trend.map(t => t.views), itemStyle: { color: '#409eff' } },
      { name: '访客数', type: 'line', smooth: true, data: trend.map(t => t.visitors), itemStyle: { color: '#67c23a' } },
    ],
  })
  window.addEventListener('resize', () => chart.resize())
}

function renderPopularChart() {
  if (!popularChartRef.value) return
  const chart = echarts.init(popularChartRef.value)
  const pages = stats.value.popularPages
  chart.setOption({
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'value', minInterval: 1 },
    yAxis: {
      type: 'category',
      data: pages.map(p => p.pagePath).reverse(),
      axisLabel: { fontSize: 11, width: 140, overflow: 'truncate' },
    },
    series: [{ type: 'bar', data: pages.map(p => p.count).reverse(), itemStyle: { color: '#409eff' } }],
  })
  window.addEventListener('resize', () => chart.resize())
}

function renderDeviceChart() {
  if (!deviceChartRef.value) return
  const chart = echarts.init(deviceChartRef.value)
  const data = stats.value.deviceDistribution
  if (data.length === 0) {
    chart.setOption({ title: { text: '暂无数据', left: 'center', top: 'center' } })
    return
  }
  const colors: Record<string, string> = {
    '桌面端': '#409eff',
    '移动端': '#67c23a',
    '平板端': '#e6a23c',
    '其他': '#909399',
  }
  chart.setOption({
    tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
    series: [{
      type: 'pie',
      radius: ['40%', '70%'],
      center: ['50%', '50%'],
      data: data.map((d: DeviceDistributionItem) => ({
        name: d.deviceType,
        value: d.count,
        itemStyle: { color: colors[d.deviceType] || '#409eff' },
      })),
      label: { formatter: '{b}\n{d}%' },
      emphasis: { itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0,0,0,0.5)' } },
    }],
  })
  window.addEventListener('resize', () => chart.resize())
}

function renderCommentChart() {
  if (!commentChartRef.value) return
  const chart = echarts.init(commentChartRef.value)
  const data = stats.value.commentTrend
  if (data.length === 0) {
    chart.setOption({ title: { text: '暂无数据', left: 'center', top: 'center' } })
    return
  }
  chart.setOption({
    tooltip: { trigger: 'axis' },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'category', data: data.map((d: CommentTrendItem) => d.date.slice(5)), boundaryGap: false },
    yAxis: { type: 'value', minInterval: 1 },
    series: [{
      name: '评论数',
      type: 'bar',
      data: data.map((d: CommentTrendItem) => d.count),
      itemStyle: { color: '#f56c6c', borderRadius: [4, 4, 0, 0] },
    }],
  })
  window.addEventListener('resize', () => chart.resize())
}

onMounted(fetchStats)
</script>

<style scoped>
.stats-page {
  padding: 20px;
}
.page-header {
  margin-bottom: 16px;
}
.page-header h3 {
  margin: 0;
  font-size: 18px;
}
.stat-cards .el-card {
  text-align: center;
}
.stat-label {
  font-size: 14px;
  color: #999;
  margin-bottom: 8px;
}
.stat-value {
  font-size: 32px;
  font-weight: bold;
  color: #303133;
}
.stat-sub {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}
</style>