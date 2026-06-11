"use client"

import { useState, useEffect, useCallback } from 'react'
import { fetchMessages } from '@/lib/api'
import type { MessageItem } from '@/lib/api'
import GuestbookMessageForm from './GuestbookMessageForm'

interface GuestbookMessageListProps {
  pageSize?: number
}

export default function GuestbookMessageList({ pageSize = 20 }: GuestbookMessageListProps) {
  const [messages, setMessages] = useState<MessageItem[]>([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(0)
  const [loading, setLoading] = useState(true)
  const [replyTo, setReplyTo] = useState<string | null>(null)

  const loadMessages = useCallback(async () => {
    setLoading(true)
    try {
      const res = await fetchMessages(page, pageSize)
      setMessages(res.data.items)
      setTotalPages(res.data.totalPages)
    } catch { /* ignore */ }
    setLoading(false)
  }, [page, pageSize])

  useEffect(() => { loadMessages() }, [loadMessages])

  const handleReplySuccess = () => {
    setReplyTo(null)
    loadMessages()
  }

  if (loading && messages.length === 0) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map(i => (
          <div key={i} className="skeleton h-[80px] rounded-[10px]" />
        ))}
      </div>
    )
  }

  if (messages.length === 0) {
    return (
      <div className="text-center py-12" style={{ color: 'var(--color-text-secondary, #6b7280)' }}>
        <svg className="w-12 h-12 mx-auto mb-3 opacity-40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
        </svg>
        <p className="text-[14px]">还没有留言，来写第一条吧！</p>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {messages.map(msg => (
        <MessageCard
          key={msg.id}
          message={msg}
          onReply={() => setReplyTo(replyTo === msg.id ? null : msg.id)}
          isReplying={replyTo === msg.id}
          onReplySuccess={handleReplySuccess}
        />
      ))}

      {totalPages > 1 && (
        <div className="flex justify-center gap-2 pt-4">
          <button
            onClick={() => setPage(p => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="px-3 py-1.5 text-[12px] rounded-[6px] border disabled:opacity-40"
            style={{ borderColor: 'var(--color-border, #e5e7eb)' }}
          >
            上一页
          </button>
          <span className="px-3 py-1.5 text-[12px] flex items-center" style={{ color: 'var(--color-text-secondary, #6b7280)' }}>
            {page} / {totalPages}
          </span>
          <button
            onClick={() => setPage(p => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages}
            className="px-3 py-1.5 text-[12px] rounded-[6px] border disabled:opacity-40"
            style={{ borderColor: 'var(--color-border, #e5e7eb)' }}
          >
            下一页
          </button>
        </div>
      )}
    </div>
  )
}

function MessageCard({ message, onReply, isReplying, onReplySuccess }: {
  message: MessageItem
  onReply: () => void
  isReplying: boolean
  onReplySuccess: () => void
}) {
  const date = new Date(message.createdAt)
  const displayDate = date.toLocaleDateString('zh-CN', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit',
  })

  return (
    <div
      className="rounded-[10px] p-4"
      style={{
        border: '0.5px solid var(--color-border, #e5e7eb)',
        backgroundColor: 'var(--card-bg, #fff)',
      }}
    >
      <div className="flex items-start gap-3">
        <div
          className="w-[36px] h-[36px] rounded-full flex items-center justify-center text-[14px] font-bold flex-shrink-0"
          style={{ backgroundColor: '#6366f1', color: '#fff' }}
        >
          {message.nickname.charAt(0).toUpperCase()}
        </div>

        <div className="flex-1 min-w-0">
          {/* Header row: nickname + ip + date + reply button */}
          <div className="flex items-center gap-2 mb-1">
            <span className="text-[13px] font-medium" style={{ color: 'var(--color-text-primary, #1f2937)' }}>
              {message.nickname}
            </span>
            {message.ipCity && (
              <span className="text-[10px] px-1.5 py-0.5 rounded" style={{ backgroundColor: '#f3f4f6', color: '#9ca3af' }}>
                {message.ipCity}
              </span>
            )}
            <span className="text-[11px] ml-auto" style={{ color: 'var(--color-text-tertiary, #9ca3af)' }}>
              {displayDate}
            </span>
            <button
              onClick={onReply}
              className="text-[11px] ml-2"
              style={{ color: 'var(--color-primary, #6366f1)' }}
            >
              {isReplying ? '取消回复' : '回复'}
            </button>
          </div>

          {/* Content */}
          <p className="text-[13px] leading-relaxed whitespace-pre-wrap" style={{ color: 'var(--color-text-primary, #1f2937)' }}>
            {message.content}
          </p>

          {/* Admin Reply */}
          {message.adminReply && (
            <div className="mt-3 p-3 rounded-[8px]" style={{ backgroundColor: '#f0fdf4', border: '0.5px solid #bbf7d0' }}>
              <div className="text-[11px] font-medium mb-1" style={{ color: '#166534' }}>
                管理员回复 {message.adminRepliedAt && `(${new Date(message.adminRepliedAt).toLocaleDateString('zh-CN')})`}
              </div>
              <p className="text-[12px] leading-relaxed" style={{ color: '#374151' }}>
                {message.adminReply}
              </p>
            </div>
          )}

          {/* Child replies (recursive) */}
          {message.children && message.children.length > 0 && (
            <div className="mt-3 space-y-2 pl-3 border-l-2" style={{ borderColor: 'var(--color-border, #e5e7eb)' }}>
              {message.children.map(child => (
                <ChildReply key={child.id} message={child} depth={1} />
              ))}
            </div>
          )}

          {/* Reply form */}
          {isReplying && (
            <div className="mt-3 pt-3 border-t" style={{ borderColor: 'var(--color-border, #e5e7eb)' }}>
              <GuestbookMessageForm parentId={message.id} onSuccess={onReplySuccess} compact />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

/** 递归渲染子留言 */
function ChildReply({ message, depth }: { message: MessageItem; depth: number }) {
  return (
    <div className="py-1">
      <div className="text-[12px] flex items-center gap-1 flex-wrap">
        <span className="font-medium" style={{ color: 'var(--color-primary, #6366f1)' }}>
          {message.nickname}
        </span>
        <span className="mx-0.5" style={{ color: 'var(--color-text-tertiary, #9ca3af)' }}>:</span>
        <span style={{ color: 'var(--color-text-primary, #1f2937)' }}>{message.content}</span>
      </div>

      {/* Admin Reply */}
      {message.adminReply && (
        <div className="mt-2 p-2 rounded-[6px]" style={{ backgroundColor: '#f0fdf4', border: '0.5px solid #bbf7d0' }}>
          <div className="text-[10px] font-medium mb-0.5" style={{ color: '#166534' }}>
            管理员回复 {message.adminRepliedAt && `(${new Date(message.adminRepliedAt).toLocaleDateString('zh-CN')})`}
          </div>
          <p className="text-[11px] leading-relaxed" style={{ color: '#374151' }}>
            {message.adminReply}
          </p>
        </div>
      )}

      {message.children && message.children.length > 0 && (
        <div className="ml-3 mt-1 space-y-1 pl-2 border-l-2" style={{ borderColor: 'var(--color-border, #e5e7eb)' }}>
          {message.children.map(child => (
            <ChildReply key={child.id} message={child} depth={depth + 1} />
          ))}
        </div>
      )}
    </div>
  )
}