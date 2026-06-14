'use client'

import { useState } from 'react'
import { getClientApiBaseUrl } from '@/lib/runtimeConfig'

export default function SubscribeForm() {
  const [email, setEmail] = useState('')
  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState('')
  const [messageType, setMessageType] = useState<'success' | 'error'>('success')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setMessage('')

    try {
      const res = await fetch(
        `${getClientApiBaseUrl()}/subscribe`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email }),
        }
      )
      const data = await res.json()
      if (data.success) {
        setMessage(data.message || '订阅成功！我们会定期推送优质内容')
        setMessageType('success')
        setEmail('')
      } else {
        setMessage(data.message || '订阅失败，请稍后重试')
        setMessageType('error')
      }
    } catch {
      setMessage('网络错误，请稍后重试')
      setMessageType('error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <section
      className="mt-10 rounded-[12px] p-6 text-center"
      style={{
        border: '0.5px solid var(--color-border)',
        backgroundColor: 'var(--color-surface-secondary, #F8FAFC)',
      }}
    >
      <h2
        className="text-[15px] font-semibold mb-1"
        style={{ color: 'var(--color-text-primary)' }}
      >
        订阅更新
      </h2>
      <p
        className="text-[12px] mb-4"
        style={{ color: 'var(--color-text-tertiary)' }}
      >
        不错过每一篇新文章 · 每周一封，随时退订
      </p>

      <form onSubmit={handleSubmit} className="flex flex-col sm:flex-row gap-2 max-w-[400px] mx-auto">
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="输入邮箱地址..."
          required
          className="flex-1 px-4 py-2 rounded-[8px] text-[13px] outline-none transition-all"
          style={{
            border: '0.5px solid var(--color-border)',
            backgroundColor: 'var(--color-surface)',
            color: 'var(--color-text-primary)',
          }}
          onFocus={(e) => {
            e.target.style.borderColor = 'var(--color-primary)'
          }}
          onBlur={(e) => {
            e.target.style.borderColor = 'var(--color-border)'
          }}
        />
        <button
          type="submit"
          disabled={loading}
          className="px-5 py-2 rounded-[8px] text-[12px] font-medium transition-opacity hover:opacity-90 disabled:opacity-50 whitespace-nowrap"
          style={{
            backgroundColor: 'var(--color-primary, #6366F1)',
            color: '#fff',
          }}
        >
          {loading ? '提交中...' : '订阅'}
        </button>
      </form>

      {message && (
        <p
          className={`mt-3 text-[11px] ${
            messageType === 'success' ? 'text-green-600' : 'text-red-500'
          }`}
        >
          {message}
        </p>
      )}
    </section>
  )
}