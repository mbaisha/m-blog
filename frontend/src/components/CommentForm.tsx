"use client";

import { useState, useCallback, useEffect } from "react";
import { fetchCaptcha, submitComment } from "@/lib/api";
import type { CaptchaImageResponse, CreateCommentResult } from "@/types";

interface CommentFormProps {
  articleId: string;
  parentId?: string | null;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export default function CommentForm({ articleId, parentId, onSuccess, onCancel }: CommentFormProps) {
  const [nickname, setNickname] = useState("");
  const [email, setEmail] = useState("");
  const [website, setWebsite] = useState("");
  const [content, setContent] = useState("");
  const [captcha, setCaptcha] = useState<CaptchaImageResponse | null>(null);
  const [captchaAnswer, setCaptchaAnswer] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [loadingCaptcha, setLoadingCaptcha] = useState(false);
  const [pendingReview, setPendingReview] = useState(false);

  // 打开时自动加载缓存的昵称、邮箱、网站
  useEffect(() => {
    try {
      const saved = localStorage.getItem('blog_user_info')
      if (saved) {
        const info = JSON.parse(saved)
        if (info.nickname) setNickname(info.nickname)
        if (info.email) setEmail(info.email)
        if (info.website) setWebsite(info.website)
      }
    } catch { /* ignore */ }
  }, [])

  const loadCaptcha = useCallback(async () => {
    setLoadingCaptcha(true);
    try {
      const res = await fetchCaptcha();
      setCaptcha(res.data);
      setCaptchaAnswer("");
    } catch {
      setError("验证码加载失败，请稍后重试");
    } finally {
      setLoadingCaptcha(false);
    }
  }, []);

  useEffect(() => {
    loadCaptcha();
  }, [loadCaptcha]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!nickname.trim()) {
      setError("请输入昵称");
      return;
    }
    if (!content.trim() || content.trim().length < 5) {
      setError("评论内容至少 5 个字符");
      return;
    }
    if (!captchaAnswer.trim()) {
      setError("请输入验证码");
      return;
    }

    setSubmitting(true);
    try {
      const res = await submitComment({
        articleId,
        parentId: parentId || undefined,
        nickname: nickname.trim(),
        email: email.trim() || undefined,
        website: website.trim() || undefined,
        content: content.trim(),
        captchaSessionId: captcha?.sessionId || "",
        captchaAnswer: captchaAnswer.trim(),
      });

      if (res.data.status === "error") {
        setError(res.data.message);
        loadCaptcha();
      } else {
        // 保存昵称、邮箱、网站到本地缓存
        try { localStorage.setItem('blog_user_info', JSON.stringify({ nickname: nickname.trim(), email: email.trim(), website: website.trim() })) } catch { /* ignore */ }

        // 评论提交成功（需审核）
        setPendingReview(true);
        setCaptchaAnswer("");
        loadCaptcha();
        // 延迟执行 onSuccess，让用户看到审核提示后再关闭表单
        setTimeout(() => {
          onSuccess?.();
        }, 3000);
      }
    } catch (err: any) {
      setError(err?.message || "提交失败，请稍后重试");
      loadCaptcha();
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <>
      {pendingReview ? (
        <div className="comment-pending-review">
          <div className="comment-pending-icon">✓</div>
          <p className="comment-pending-title">评论已提交</p>
          <p className="comment-pending-desc">您的评论已成功提交，正在等待审核。审核通过后即可显示。</p>
        </div>
      ) : (
      <form onSubmit={handleSubmit} className="comment-form">
      {error && (
        <div className="comment-form-error">
          {error}
        </div>
      )}

      <div className="comment-form-row">
        <div className="comment-form-group flex-1">
          <label htmlFor="nickname" className="comment-form-label">
            昵称 <span className="text-red-500">*</span>
          </label>
          <input
            id="nickname"
            type="text"
            value={nickname}
            onChange={(e) => setNickname(e.target.value)}
            placeholder="你的昵称"
            maxLength={64}
            className="comment-form-input"
            required
          />
        </div>
        <div className="comment-form-group flex-1">
          <label htmlFor="email" className="comment-form-label">邮箱</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="接收回复通知（可选）"
            className="comment-form-input"
          />
        </div>
        <div className="comment-form-group flex-1">
          <label htmlFor="website" className="comment-form-label">网站</label>
          <input
            id="website"
            type="url"
            value={website}
            onChange={(e) => setWebsite(e.target.value)}
            placeholder="你的网站（可选）"
            className="comment-form-input"
          />
        </div>
      </div>

      <div className="comment-form-group">
        <label htmlFor="content" className="comment-form-label">
          评论内容 <span className="text-red-500">*</span>
        </label>
        <textarea
          id="content"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="写下你的评论...（至少 5 个字符）"
          rows={4}
          maxLength={1000}
          className="comment-form-textarea"
          required
        />
      </div>

      {/* 验证码 + 操作按钮：全部一行 */}
      <div className="comment-form-captcha-actions" style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
        {captcha && (
          <img
            src={captcha.imageBase64}
            alt="验证码"
            className="captcha-image"
            onClick={loadCaptcha}
            style={{ cursor: "pointer", height: '40px', borderRadius: '6px' }}
          />
        )}
        <input
          id="captcha"
          type="text"
          value={captchaAnswer}
          onChange={(e) => setCaptchaAnswer(e.target.value)}
          placeholder="验证码"
          maxLength={6}
          style={{ width: '90px' }}
          className="comment-form-input"
          required
        />
        <button
          type="button"
          onClick={loadCaptcha}
          disabled={loadingCaptcha}
          className="captcha-refresh-btn"
          title="刷新验证码"
          style={{ fontSize: '11px', whiteSpace: 'nowrap' }}
        >
          {loadingCaptcha ? "加载中..." : "换一张"}
        </button>
        <button
          type="submit"
          disabled={submitting || loadingCaptcha}
          className="comment-btn-submit"
        >
          {submitting ? "提交中..." : "提交评论"}
        </button>
        {parentId && onCancel && (
          <button type="button" onClick={onCancel} className="comment-btn-cancel">
            取消回复
          </button>
        )}
      </div>
    </form>
      )}
    </>
  );
}