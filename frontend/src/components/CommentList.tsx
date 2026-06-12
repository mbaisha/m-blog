"use client";

import { useState, useEffect, useCallback } from "react";
import Image from "next/image";
import { fetchComments } from "@/lib/api";
import type { CommentItem, PagedData } from "@/types";
import CommentForm from "./CommentForm";

interface CommentListProps {
  articleId: string;
}

function formatDateTime(dateStr: string): string {
  const d = new Date(dateStr);
  return d.toLocaleString("zh-CN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function CommentAvatar({ nickname, avatar }: { nickname: string; avatar: string | null }) {
  return (
    <div className="comment-avatar">
      {avatar ? (
        <Image src={avatar} alt={nickname} width={40} height={40} className="comment-avatar-img" />
      ) : (
        <div className="comment-avatar-placeholder">
          {nickname.charAt(0).toUpperCase()}
        </div>
      )}
    </div>
  );
}

function CommentCard({
  comment,
  onReply,
}: {
  comment: CommentItem;
  onReply: (id: string, nickname: string) => void;
}) {
  return (
    <div className="comment-card" id={`comment-${comment.id}`}>
      <CommentAvatar nickname={comment.nickname} avatar={comment.avatar} />
      <div className="comment-body">
        <div className="comment-header">
          <span className="comment-nickname">
            {comment.website ? (
              <a href={comment.website} target="_blank" rel="nofollow noopener noreferrer">
                {comment.nickname}
              </a>
            ) : (
              comment.nickname
            )}
          </span>
          <span className="comment-time">{formatDateTime(comment.createdAt)}</span>
        </div>
        {/* 回复上下文 */}
        {comment.parentId && comment.parentNickname && (
          <div className="comment-reply-context">
            <span className="comment-reply-arrow">↳</span>
            <span className="comment-reply-nickname">{comment.parentNickname}：</span>
            <span className="comment-reply-preview">{comment.parentContent}</span>
          </div>
        )}
        <div className="comment-content">{comment.content}</div>
        <div className="comment-actions">
          <button
            className="comment-reply-btn"
            onClick={() => onReply(comment.id, comment.nickname)}
          >
            回复
          </button>
        </div>
      </div>
    </div>
  );
}

export default function CommentList({ articleId }: CommentListProps) {
  const [comments, setComments] = useState<CommentItem[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [replyTo, setReplyTo] = useState<string | null>(null);
  const [replyToNickname, setReplyToNickname] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);

  const loadComments = useCallback(async (p: number) => {
    setLoading(true);
    try {
      const res = await fetchComments(articleId, p);
      const data: PagedData<CommentItem> = res.data;
      setComments(data.items);
      setTotal(data.totalCount);
      setPage(data.page);
      setTotalPages(data.totalPages);
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  }, [articleId]);

  useEffect(() => {
    loadComments(1);
  }, [loadComments]);

  const handleReply = (id: string, nickname: string) => {
    setReplyTo(id);
    setReplyToNickname(nickname);
    setShowForm(true);
    document.getElementById("comment-form-section")?.scrollIntoView({ behavior: "smooth" });
  };

  const handleNewComment = () => {
    setReplyTo(null);
    setReplyToNickname(null);
    setShowForm(true);
  };

  const handleSuccess = () => {
    setReplyTo(null);
    setReplyToNickname(null);
    setShowForm(false);
    loadComments(1);
  };

  return (
    <section className="comment-section">
      <div className="comment-section-header">
        <h2 className="comment-section-title">
          评论
          {total > 0 && <span className="comment-count"> ({total})</span>}
        </h2>
        {!showForm && (
          <button className="comment-new-btn" onClick={handleNewComment}>
            写评论
          </button>
        )}
      </div>

      {/* 评论表单 */}
      {showForm && (
        <div id="comment-form-section" className="comment-form-section">
          <CommentForm
            articleId={articleId}
            parentId={replyTo}
            onSuccess={handleSuccess}
            onCancel={() => {
              setReplyTo(null);
              setShowForm(false);
            }}
          />
        </div>
      )}

      {/* 回复提示 */}
      {replyTo && replyToNickname && (
        <div className="reply-indicator">
          正在回复 <strong>{replyToNickname}</strong>
          <button onClick={() => { setReplyTo(null); setReplyToNickname(null); }} className="reply-cancel">
            取消
          </button>
        </div>
      )}

      {/* 评论列表 */}
      {loading ? (
        <div className="comment-loading">加载中...</div>
      ) : comments.length === 0 ? (
        <div className="comment-empty">
          暂无评论，快来抢沙发吧~
        </div>
      ) : (
        <div className="comment-list">
          {comments.map((comment) => (
            <CommentCard
              key={comment.id}
              comment={comment}
              onReply={handleReply}
            />
          ))}
        </div>
      )}

      {/* 分页 */}
      {totalPages > 1 && (
        <div className="comment-pagination">
          <button
            disabled={page <= 1}
            onClick={() => loadComments(page - 1)}
            className="comment-page-btn"
          >
            上一页
          </button>
          <span className="comment-page-info">
            {page} / {totalPages}
          </span>
          <button
            disabled={page >= totalPages}
            onClick={() => loadComments(page + 1)}
            className="comment-page-btn"
          >
            下一页
          </button>
        </div>
      )}
    </section>
  );
}