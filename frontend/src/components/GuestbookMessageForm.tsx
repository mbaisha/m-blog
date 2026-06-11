"use client"

import { useState, useEffect } from 'react'
import { fetchCaptcha, submitMessage } from '@/lib/api'
import type { CreateMessageRequest } from '@/lib/api'

interface GuestbookMessageFormProps {
  parentId?: string
  onSuccess?: () => void
  compact?: boolean
}

export default function GuestbookMessageForm({ parentId, onSuccess, compact }: GuestbookMessageFormProps) {
  const [nickname, setNickname] = useState('')
  const [email, setEmail] = useState('')
  const [content, setContent] = useState('')
  const [captchaSessionId, setCaptchaSessionId] = useState('')
  const [captchaImage, setCaptchaImage] = useState('')
  const [captchaAnswer, setCaptchaAnswer] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [message, setMessage] = useState('')
  const [hidden, setHidden] = useState(false)

  // 打开时自动加载缓存的昵称与邮箱
  useEffect(() => {
    try {
      const saved = localStorage.getItem('blog_user_info')
      if (saved) {
        const info = JSON.parse(saved)
        if (info.nickname) setNickname(info.nickname)
        if (info.email) setEmail(info.email)
      }
    } catch { /* ignore */ }
  }, [])

  useEffect(() => { loadCaptcha() }, [])

  const loadCaptcha = async () => {
    try {
      const res = await fetchCaptcha()
      setCaptchaSessionId(res.data.sessionId)
      setCaptchaImage(res.data.imageBase64)
    } catch { /* ignore */ }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!nickname.trim() || !content.trim() || !captchaAnswer.trim()) return

    setSubmitting(true)
    setMessage('')

    try {
      const data: CreateMessageRequest = {
        nickname: nickname.trim(),
        email: email.trim() || undefined,
        content: content.trim(),
        parentId,
        captchaSessionId,
        captchaAnswer: captchaAnswer.trim(),
      }
      await submitMessage(data)
      // 保存昵称与邮箱到本地缓存
      try { localStorage.setItem('blog_user_info', JSON.stringify({ nickname: nickname.trim(), email: email.trim() })) } catch { /* ignore */ }

      // 立即隐藏表单
      setHidden(true)

      // 显示成功提示，2 秒后消失并回调
      setMessage('留言已提交，等待审核后显示')
      setTimeout(() => {
        setMessage('')
        if (onSuccess) onSuccess()
      }, 2000)
    } catch (err: any) {
      setMessage(err?.message || '提交失败，请重试')
      loadCaptcha()
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <>
      {message && (
        <div className="p-3 rounded-[8px] text-[13px] mb-4" style={{
          backgroundColor: message.includes('失败') ? '#fef2f2' : '#f0fdf4',
          color: message.includes('失败') ? '#dc2626' : '#166534',
          border: `0.5px solid ${message.includes('失败') ? '#fecaca' : '#bbf7d0'}`,
        }}>
          {message}
        </div>
      )}

      {!hidden && (
      <form onSubmit={handleSubmit}>

      <div className="flex flex-col sm:flex-row gap-3 mb-4">
        <input
          type="text"
          value={nickname}
          onChange={e => setNickname(e.target.value)}
          placeholder="昵称 *"
          required
          className="flex-1 px-3 py-2 text-[13px] rounded-[8px] border"
          style={{ borderColor: 'var(--color-border, #e5e7eb)', backgroundColor: 'var(--card-bg, #fff)', color: 'var(--color-text-primary, #1f2937)' }}
        />
        <input
          type="email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          placeholder="邮箱（选填）"
          className="flex-1 px-3 py-2 text-[13px] rounded-[8px] border"
          style={{ borderColor: 'var(--color-border, #e5e7eb)', backgroundColor: 'var(--card-bg, #fff)', color: 'var(--color-text-primary, #1f2937)' }}
        />
      </div>

      <textarea
        value={content}
        onChange={e => setContent(e.target.value)}
        placeholder="写下你的留言..."
        rows={compact ? 3 : 4}
        required
        className="w-full px-3 py-2 text-[13px] rounded-[8px] border resize-none mb-4"
        style={{ borderColor: 'var(--color-border, #e5e7eb)', backgroundColor: 'var(--card-bg, #fff)', color: 'var(--color-text-primary, #1f2937)' }}
      />

      <div className="flex items-center gap-3 flex-wrap">
        {captchaImage && (
          <img
            src={captchaImage}
            alt="验证码"
            className="h-[40px] rounded-[6px] cursor-pointer"
            onClick={loadCaptcha}
          />
        )}
        <input
          type="text"
          value={captchaAnswer}
          onChange={e => setCaptchaAnswer(e.target.value)}
          placeholder="验证码 *"
          required
          maxLength={4}
          className="w-[100px] px-3 py-2 text-[13px] rounded-[8px] border"
          style={{ borderColor: 'var(--color-border, #e5e7eb)', backgroundColor: 'var(--card-bg, #fff)', color: 'var(--color-text-primary, #1f2937)' }}
        />
        <button
          type="button"
          onClick={loadCaptcha}
          className="text-[11px] px-2 py-1 rounded"
          style={{ color: 'var(--color-primary, #6366f1)' }}
        >
          换一张
        </button>
        <button
          type="submit"
          disabled={submitting}
          className="px-5 py-2 rounded-[8px] text-[13px] font-medium text-white transition-opacity hover:opacity-90 disabled:opacity-50"
          style={{ backgroundColor: 'var(--color-primary, #6366f1)' }}
        >
          {submitting ? '提交中...' : compact ? '回复' : '提交留言'}
        </button>
      </div>
    </form>
      )}
    </>
  )
}