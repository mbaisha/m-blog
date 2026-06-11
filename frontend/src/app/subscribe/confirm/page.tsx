'use client';

import { useEffect, useState, use } from 'react';
import Link from 'next/link';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5092/api';

type Status = 'loading' | 'success' | 'expired' | 'invalid' | 'error';

export default function ConfirmSubscriptionPage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const params = use(searchParams);
  const [status, setStatus] = useState<Status>('loading');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = params.token;
    if (!token) {
      setStatus('invalid');
      setMessage('缺少确认令牌');
      return;
    }

    fetch(`${API_BASE_URL}/subscribe/confirm?token=${encodeURIComponent(token)}`)
      .then(async (res) => {
        const data = await res.json();
        if (res.ok && data.success) {
          setStatus('success');
          setMessage(data.message || '订阅确认成功！');
        } else if (res.status === 400) {
          const msg = data.message || '确认链接无效或已过期';
          if (msg.includes('过期')) {
            setStatus('expired');
          } else if (msg.includes('已经确认')) {
            setStatus('success');
          } else {
            setStatus('invalid');
          }
          setMessage(msg);
        } else {
          setStatus('error');
          setMessage(data.message || '确认失败，请稍后重试');
        }
      })
      .catch(() => {
        setStatus('error');
        setMessage('网络错误，请稍后重试');
      });
  }, [params.token]);

  const statusConfig: Record<Status, { icon: string; title: string; color: string; bgColor: string }> = {
    loading: {
      icon: '⟳',
      title: '正在确认...',
      color: 'var(--color-text-secondary)',
      bgColor: 'var(--color-surface)',
    },
    success: {
      icon: '✓',
      title: '订阅成功',
      color: '#10b981',
      bgColor: '#ecfdf5',
    },
    expired: {
      icon: '!',
      title: '链接已过期',
      color: '#f59e0b',
      bgColor: '#fffbeb',
    },
    invalid: {
      icon: '✕',
      title: '无效链接',
      color: '#ef4444',
      bgColor: '#fef2f2',
    },
    error: {
      icon: '✕',
      title: '确认失败',
      color: '#ef4444',
      bgColor: '#fef2f2',
    },
  };

  const config = statusConfig[status];

  return (
    <div className="min-h-[60vh] flex items-center justify-center px-4 py-16">
      <div
        className="w-full max-w-[480px] rounded-[16px] p-10 text-center"
        style={{
          border: '0.5px solid var(--color-border)',
          backgroundColor: 'var(--color-surface)',
        }}
      >
        {status === 'loading' ? (
          <div className="flex flex-col items-center gap-4">
            <div
              className="w-16 h-16 rounded-full flex items-center justify-center text-3xl animate-spin"
              style={{ color: 'var(--color-primary)', border: '3px solid var(--color-border)', borderTopColor: 'var(--color-primary)' }}
            >
              &nbsp;
            </div>
            <p style={{ color: 'var(--color-text-secondary)', fontSize: '15px' }}>
              正在验证您的订阅...
            </p>
          </div>
        ) : (
          <>
            <div
              className="w-16 h-16 rounded-full flex items-center justify-center text-3xl font-bold mx-auto mb-6"
              style={{ backgroundColor: config.bgColor, color: config.color }}
            >
              {config.icon}
            </div>

            <h1
              className="text-xl font-semibold mb-3"
              style={{ color: config.color }}
            >
              {config.title}
            </h1>

            <p
              className="text-[14px] mb-8 leading-relaxed"
              style={{ color: 'var(--color-text-secondary)' }}
            >
              {message}
            </p>

            {status === 'success' && (
              <Link
                href="/"
                className="inline-block px-8 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                style={{
                  backgroundColor: 'var(--color-primary)',
                  color: '#fff',
                }}
              >
                返回首页
              </Link>
            )}

            {status === 'expired' && (
              <Link
                href="/"
                className="inline-block px-8 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                style={{
                  backgroundColor: 'var(--color-primary)',
                  color: '#fff',
                }}
              >
                重新订阅
              </Link>
            )}

            {(status === 'invalid' || status === 'error') && (
              <Link
                href="/"
                className="inline-block px-6 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                style={{
                  backgroundColor: 'var(--color-surface)',
                  color: 'var(--color-text-secondary)',
                  border: '0.5px solid var(--color-border)',
                }}
              >
                返回首页
              </Link>
            )}
          </>
        )}
      </div>
    </div>
  );
}