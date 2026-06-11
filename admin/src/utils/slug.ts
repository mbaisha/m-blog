/**
 * URL 标识自动生成工具
 * - 中文转为拼音（无声调）
 * - 英文/数字保留原样，空格转为连字符
 * - 全部小写
 * - 连续连字符合并
 * - 首尾连字符去除
 */
import { pinyin } from 'pinyin-pro'

/**
 * 将文本分段，中文部分转拼音，非中文保留原样
 * 避免 pinyin-pro 将英文逐字拆分（"Blog" → "B l o g"）
 */
export function toSlug(text: string): string {
  if (!text) return ''

  // 按中文/非中文分段处理
  const segments = text
    .split(/([\u4e00-\u9fa5]+)/)
    .filter(Boolean)
    .map(segment => {
      if (/^[\u4e00-\u9fa5]+$/.test(segment)) {
        // 纯中文段 → 转拼音（无声调，空格分隔）
        return pinyin(segment, {
          toneType: 'none',
          type: 'string',
          separator: ' '
        })
      }
      // 非中文段（英文/数字/符号）→ 保留原样
      return segment
    })

  // 用空格拼接各段，避免中文拼音与英文粘连（如 "我的Blog" → "wo de Blog"）
  const result = segments.join(' ')

  // 统一处理：小写 → 非字母数字 → 连字符 → 合并 → 去首尾
  return result
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+/, '')
    .replace(/-+$/, '')
}