'use client';

import { Suspense, useState, useMemo } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5092/api';

type Step = 'prefix' | 'verify' | 'done' | 'subscribe';

export default function UnsubscribePageWrapper() {
  return (
    <Suspense fallback={
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center">
          <div className="w-16 h-16 rounded-full bg-gray-100 animate-pulse mx-auto mb-4" />
          <div className="h-6 w-48 bg-gray-100 animate-pulse mx-auto rounded" />
        </div>
      </div>
    }>
      <UnsubscribePage />
    </Suspense>
  );
}

function UnsubscribePage() {
  const searchParams = useSearchParams();
  const fullEmail = searchParams.get('email') || '';

  const { domainPart, localPart } = useMemo(() => {
    const atIndex = fullEmail.indexOf('@');
    if (atIndex > 0) {
      return {
        domainPart: fullEmail.slice(atIndex),
        localPart: fullEmail.slice(0, atIndex),
      };
    }
    return { domainPart: '', localPart: '' };
  }, [fullEmail]);

  const [step, setStep] = useState<Step>('prefix');
  const [prefixInput, setPrefixInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [sessionId, setSessionId] = useState('');
  const [maskedEmail, setMaskedEmail] = useState('');
  const [code, setCode] = useState('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState<'success' | 'error'>('success');

  // 重新订阅表单
  const [subEmail, setSubEmail] = useState('');
  const [subLoading, setSubLoading] = useState(false);
  const [subMessage, setSubMessage] = useState('');
  const [subDone, setSubDone] = useState(false);

  const handleRequestCaptcha = async (e: React.FormEvent) => {
    e.preventDefault();
    const reconstructedEmail = `${prefixInput.trim()}${domainPart}`;
    if (!prefixInput.trim() || !reconstructedEmail) return;

    setLoading(true);
    setMessage('');

    try {
      const res = await fetch(`${API_BASE_URL}/unsubscribe/captcha`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: reconstructedEmail.toLowerCase() }),
      });
      const data = await res.json();

      if (res.ok && data.success) {
        setSessionId(data.data.sessionId);
        setMaskedEmail(data.data.maskedEmail);
        setStep('verify');
        setMessageType('success');
        setMessage('验证码已发送至您的邮箱，请查收');
      } else {
        setMessageType('error');
        setMessage(data.message || '未找到匹配的订阅邮箱');
      }
    } catch {
      setMessageType('error');
      setMessage('网络错误，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmUnsubscribe = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!code.trim()) return;

    setLoading(true);
    setMessage('');

    try {
      const res = await fetch(`${API_BASE_URL}/unsubscribe/confirm`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sessionId, code: code.trim() }),
      });
      const data = await res.json();

      if (res.ok && data.success) {
        setStep('done');
        setMessageType('success');
        setMessage(data.message || '已成功退订');
      } else {
        setMessageType('error');
        setMessage(data.message || '验证失败，请重试');
      }
    } catch {
      setMessageType('error');
      setMessage('网络错误，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  const handleResubscribe = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!subEmail.trim()) return;

    setSubLoading(true);
    setSubMessage('');

    try {
      const res = await fetch(`${API_BASE_URL}/subscribe`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: subEmail.trim().toLowerCase() }),
      });
      const data = await res.json();

      if (res.ok && data.success) {
        setSubDone(true);
        setSubMessage(data.message || '订阅成功，请查收确认邮件');
      } else {
        setSubMessage(data.message || '订阅失败，请稍后重试');
      }
    } catch {
      setSubMessage('网络错误，请稍后重试');
    } finally {
      setSubLoading(false);
    }
  };

  const handleReset = () => {
    setPrefixInput('');
    setCode('');
    setSessionId('');
    setMaskedEmail('');
    setMessage('');
    setStep('prefix');
  };

  const handleGoResubscribe = () => {
    // 预填之前退订的邮箱
    setSubEmail(fullEmail || '');
    setSubDone(false);
    setSubMessage('');
    setStep('subscribe');
  };

  return (
    <div className="min-h-[60vh] flex items-center justify-center px-4 py-16">
      <div
        className="w-full max-w-[460px] rounded-[16px] p-8"
        style={{
          border: '0.5px solid var(--color-border)',
          backgroundColor: 'var(--color-surface)',
        }}
      >
        {step === 'done' && !subDone ? (
          <div className="text-center">
            <div
              className="w-16 h-16 rounded-full flex items-center justify-center text-3xl font-bold mx-auto mb-6"
              style={{ backgroundColor: '#ecfdf5', color: '#10b981' }}
            >
              ✓
            </div>
            <h1
              className="text-xl font-semibold mb-3"
              style={{ color: '#10b981' }}
            >
              已成功退订
            </h1>
            <p
              className="text-[14px] mb-6 leading-relaxed"
              style={{ color: 'var(--color-text-secondary)' }}
            >
              您已成功取消订阅，退订确认邮件已发送至您的邮箱。
            </p>
            <div className="flex gap-3 justify-center">
              <Link
                href="/"
                className="inline-block px-6 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                style={{
                  backgroundColor: 'var(--color-primary)',
                  color: '#fff',
                }}
              >
                返回首页
              </Link>
              <button
                onClick={handleGoResubscribe}
                className="inline-block px-6 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                style={{
                  backgroundColor: 'var(--color-surface)',
                  color: 'var(--color-primary)',
                  border: '1px solid var(--color-primary)',
                }}
              >
                重新订阅
              </button>
            </div>
          </div>
        ) : step === 'subscribe' || subDone ? (
          subDone ? (
            <div className="text-center">
              <div
                className="w-16 h-16 rounded-full flex items-center justify-center text-3xl font-bold mx-auto mb-6"
                style={{ backgroundColor: '#f0f0ff', color: '#6366f1' }}
              >
                ✉️
              </div>
              <h1
                className="text-xl font-semibold mb-3"
                style={{ color: '#6366f1' }}
              >
                订阅成功
              </h1>
              <p
                className="text-[14px] mb-6 leading-relaxed"
                style={{ color: 'var(--color-text-secondary)' }}
              >
                {subMessage}
              </p>
              <div className="flex gap-3 justify-center">
                <Link
                  href="/"
                  className="inline-block px-6 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                  style={{
                    backgroundColor: 'var(--color-primary)',
                    color: '#fff',
                  }}
                >
                  返回首页
                </Link>
                <button
                  onClick={() => { setStep('prefix'); setSubDone(false); setSubMessage(''); }}
                  className="inline-block px-6 py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90"
                  style={{
                    backgroundColor: 'var(--color-surface)',
                    color: 'var(--color-text-secondary)',
                    border: '0.5px solid var(--color-border)',
                  }}
                >
                  退订管理
                </button>
              </div>
            </div>
          ) : (
            <>
              <div className="text-center mb-8">
                <div
                  className="w-14 h-14 rounded-full flex items-center justify-center text-2xl mx-auto mb-4"
                  style={{ backgroundColor: '#f0f0ff', color: '#6366f1' }}
                >
                  📬
                </div>
                <h1
                  className="text-xl font-semibold mb-1"
                  style={{ color: 'var(--color-text-primary)' }}
                >
                  重新订阅
                </h1>
                <p
                  className="text-[13px]"
                  style={{ color: 'var(--color-text-tertiary)' }}
                >
                  输入您的邮箱地址，重新加入订阅
                </p>
              </div>

              <form onSubmit={handleResubscribe}>
                <div className="mb-6">
                  <label
                    className="block text-[13px] font-medium mb-2"
                    style={{ color: 'var(--color-text-secondary)' }}
                  >
                    邮箱地址
                  </label>
                  <input
                    type="email"
                    value={subEmail}
                    onChange={(e) => setSubEmail(e.target.value)}
                    placeholder="your@email.com"
                    required
                    className="w-full px-4 py-3 rounded-[10px] text-[14px] outline-none transition-all"
                    style={{
                      border: '0.5px solid var(--color-border)',
                      backgroundColor: 'var(--color-bg)',
                      color: 'var(--color-text-primary)',
                    }}
                    onFocus={(e) => {
                      e.target.style.borderColor = '#6366f1';
                    }}
                    onBlur={(e) => {
                      e.target.style.borderColor = 'var(--color-border)';
                    }}
                    autoFocus
                  />
                </div>

                {subMessage && (
                  <div
                    className="mb-4 px-4 py-2 rounded-[8px] text-[13px]"
                    style={{
                      backgroundColor: '#f0f0ff',
                      color: '#6366f1',
                      border: '0.5px solid #d0d0ff',
                    }}
                  >
                    {subMessage}
                  </div>
                )}

                <button
                  type="submit"
                  disabled={subLoading || !subEmail.trim()}
                  className="w-full py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90 disabled:opacity-50"
                  style={{
                    background: 'linear-gradient(135deg, #6366f1, #8b5cf6)',
                    color: '#fff',
                  }}
                >
                  {subLoading ? '提交中...' : '确认订阅'}
                </button>

                <div className="mt-4 text-center">
                  <button
                    type="button"
                    onClick={() => { setStep('prefix'); setSubMessage(''); }}
                    className="text-[13px] hover:underline"
                    style={{ color: 'var(--color-text-tertiary)' }}
                  >
                    返回退订页面
                  </button>
                </div>
              </form>
            </>
          )
        ) : (
          <>
            <div className="text-center mb-8">
              <div
                className="w-14 h-14 rounded-full flex items-center justify-center text-2xl mx-auto mb-4"
                style={{ backgroundColor: '#fef2f2', color: '#ef4444' }}
              >
                ⊘
              </div>
              <h1
                className="text-xl font-semibold mb-1"
                style={{ color: 'var(--color-text-primary)' }}
              >
                取消订阅
              </h1>
              <p
                className="text-[13px]"
                style={{ color: 'var(--color-text-tertiary)' }}
              >
                我们将不再向您推送邮件通知
              </p>
            </div>

            {step === 'prefix' && (
              <form onSubmit={handleRequestCaptcha}>
                <div className="mb-6">
                  <label
                    className="block text-[13px] font-medium mb-2"
                    style={{ color: 'var(--color-text-secondary)' }}
                  >
                    请补全您的邮箱地址以确认身份
                  </label>
                  <div
                    className="flex items-center rounded-[10px] overflow-hidden transition-all"
                    style={{
                      border: '0.5px solid var(--color-border)',
                      backgroundColor: 'var(--color-bg)',
                    }}
                  >
                    <input
                      type="text"
                      value={prefixInput}
                      onChange={(e) => setPrefixInput(e.target.value)}
                      placeholder={localPart || "邮箱前缀"}
                      required
                      className="flex-1 px-4 py-3 text-[14px] outline-none border-none"
                      style={{
                        backgroundColor: 'transparent',
                        color: 'var(--color-text-primary)',
                      }}
                      autoFocus
                    />
                    {domainPart && (
                      <span
                        className="px-4 py-3 text-[14px] select-none"
                        style={{
                          color: 'var(--color-text-tertiary)',
                          backgroundColor: 'var(--color-surface-secondary, #f8fafc)',
                          borderLeft: '0.5px solid var(--color-border)',
                        }}
                      >
                        {domainPart}
                      </span>
                    )}
                  </div>
                  {!fullEmail && (
                    <p
                      className="text-[12px] mt-1.5"
                      style={{ color: 'var(--color-text-tertiary)' }}
                    >
                      提示：请从退订邮件中复制完整邮箱地址，或输入邮箱前缀（如 abc@）
                    </p>
                  )}
                </div>

                {message && (
                  <div
                    className="mb-4 px-4 py-2 rounded-[8px] text-[13px]"
                    style={{
                      backgroundColor: messageType === 'success' ? '#ecfdf5' : '#fef2f2',
                      color: messageType === 'success' ? '#10b981' : '#ef4444',
                    }}
                  >
                    {message}
                  </div>
                )}

                <button
                  type="submit"
                  disabled={loading || !prefixInput.trim()}
                  className="w-full py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90 disabled:opacity-50"
                  style={{
                    backgroundColor: '#ef4444',
                    color: '#fff',
                  }}
                >
                  {loading ? '发送中...' : '获取验证码'}
                </button>

                <div className="mt-4 text-center">
                  <Link
                    href="/"
                    className="text-[13px] hover:underline"
                    style={{ color: 'var(--color-text-tertiary)' }}
                  >
                    返回首页
                  </Link>
                </div>
              </form>
            )}

            {step === 'verify' && (
              <form onSubmit={handleConfirmUnsubscribe}>
                <div
                  className="mb-5 px-4 py-3 rounded-[8px] text-[13px] text-center"
                  style={{
                    backgroundColor: 'var(--color-surface-secondary, #f8fafc)',
                    color: 'var(--color-text-secondary)',
                    border: '0.5px solid var(--color-border)',
                  }}
                >
                  正在退订：<strong>{maskedEmail}</strong>
                </div>

                <div className="mb-6">
                  <label
                    className="block text-[13px] font-medium mb-2"
                    style={{ color: 'var(--color-text-secondary)' }}
                  >
                    请输入邮箱中收到的验证码
                  </label>
                  <input
                    type="text"
                    value={code}
                    onChange={(e) => setCode(e.target.value)}
                    placeholder="6 位数字验证码"
                    maxLength={6}
                    required
                    className="w-full px-4 py-3 rounded-[10px] text-[14px] text-center tracking-[8px] font-bold outline-none transition-all"
                    style={{
                      border: '0.5px solid var(--color-border)',
                      backgroundColor: 'var(--color-bg)',
                      color: 'var(--color-text-primary)',
                      fontSize: '20px',
                    }}
                    onFocus={(e) => {
                      e.target.style.borderColor = 'var(--color-primary)';
                    }}
                    onBlur={(e) => {
                      e.target.style.borderColor = 'var(--color-border)';
                    }}
                  />
                </div>

                {message && (
                  <div
                    className="mb-4 px-4 py-2 rounded-[8px] text-[13px]"
                    style={{
                      backgroundColor: messageType === 'success' ? '#ecfdf5' : '#fef2f2',
                      color: messageType === 'success' ? '#10b981' : '#ef4444',
                    }}
                  >
                    {message}
                  </div>
                )}

                <button
                  type="submit"
                  disabled={loading || code.trim().length < 6}
                  className="w-full py-3 rounded-[10px] text-[14px] font-medium transition-all hover:opacity-90 disabled:opacity-50"
                  style={{
                    backgroundColor: '#ef4444',
                    color: '#fff',
                  }}
                >
                  {loading ? '验证中...' : '确认退订'}
                </button>

                <div className="mt-4 text-center">
                  <button
                    type="button"
                    onClick={handleReset}
                    className="text-[13px] hover:underline"
                    style={{ color: 'var(--color-text-tertiary)' }}
                  >
                    重新输入邮箱
                  </button>
                </div>
              </form>
            )}
          </>
        )}
      </div>
    </div>
  );
}