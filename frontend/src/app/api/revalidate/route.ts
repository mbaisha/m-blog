import { NextRequest, NextResponse } from "next/server";
import { revalidateTag, revalidatePath } from "next/cache";

/**
 * 由后端在文章/项目/分类/标签等变更后调用
 * 接收 { tags?: string[], paths?: string[], secret: string }
 * 通过 tag/path 选择性失效缓存，而不是清空全站
 */
export const runtime = "nodejs";

interface RevalidateBody {
  secret?: string;
  tags?: string[];
  paths?: string[];
}

export async function POST(request: NextRequest) {
  let body: RevalidateBody;
  try {
    body = await request.json();
  } catch {
    return NextResponse.json({ ok: false, error: "Invalid JSON" }, { status: 400 });
  }

  const expected = process.env.REVALIDATE_SECRET;
  if (!expected) {
    return NextResponse.json(
      { ok: false, error: "REVALIDATE_SECRET not configured" },
      { status: 500 },
    );
  }
  if (!body.secret || body.secret !== expected) {
    return NextResponse.json({ ok: false, error: "Unauthorized" }, { status: 401 });
  }

  const tags = Array.isArray(body.tags) ? body.tags.filter(Boolean) : [];
  const paths = Array.isArray(body.paths) ? body.paths.filter(Boolean) : [];

  for (const tag of tags) {
    try {
      // Next.js 15: revalidateTag(tag) 即可
      (revalidateTag as (t: string) => void)(tag);
    } catch (err) {
      // 单个 tag 失败不影响其他
      console.warn(`[revalidate] tag ${tag} failed:`, err);
    }
  }
  for (const path of paths) {
    try {
      (revalidatePath as (p: string) => void)(path);
    } catch (err) {
      console.warn(`[revalidate] path ${path} failed:`, err);
    }
  }

  return NextResponse.json({ ok: true, tags, paths });
}

export async function GET() {
  return NextResponse.json({ ok: false, error: "Method not allowed" }, { status: 405 });
}
