/**
 * Extract headings from markdown content for TOC generation.
 * @param markdown - Raw markdown content
 * @param skipTitles - Optional array of heading texts to exclude (e.g. article title duplicated in content)
 * Uses the same slug generation logic as rehype-slug (github-slugger).
 * Keeps only two heading levels: the topmost level present and its next level.
 *   - If h1 exists → shows h1, h2
 *   - If no h1 but h2 exists → shows h2, h3
 *   - If no h1/h2 but h3 exists → shows h3, h4
 */

export interface TocItem {
  id: string
  text: string
  level: number
}

// Simulates github-slugger behavior + handles Chinese fallback
function slugger(text: string, seen: Map<string, number>): string {
  let slug = text
    .toLowerCase()
    .trim()
    .replace(/\s+/g, '-')
    .replace(/[^\w-]/g, '')

  // If slug is empty (e.g. Chinese-only headings), use char codes as fallback
  if (!slug) {
    slug = Array.from(text)
      .slice(0, 4)
      .map((c) => c.charCodeAt(0).toString(36))
      .join('')
  }

  const count = seen.get(slug) || 0
  if (count > 0) {
    slug = `${slug}-${count}`
  }
  seen.set(slug.replace(/-\d+$/, ''), count + 1)
  return slug
}

export function extractHeadings(markdown: string, skipTitles: string[] = []): TocItem[] {
  const allHeadings: TocItem[] = []
  const seen = new Map<string, number>()
  const lines = markdown.split('\n')
  let inCodeBlock = false

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i]

    // Toggle code fence state (``` or ~~~)
    if (/^(```|~~~)/.test(line.trim())) {
      inCodeBlock = !inCodeBlock
      continue
    }

    // Skip lines inside code blocks
    if (inCodeBlock) continue

    // Match headings: line must start with 1-4 # followed by a space
    const match = line.match(/^(#{1,4})\s+(.+)$/)
    if (!match) continue

    const level = match[1].length
    const text = match[2].trim()
    // Skip empty heading text
    if (!text) continue

    // Skip headings that match any skipped title (e.g. article title duplicated in body)
    if (skipTitles.some(st => st === text)) continue

    const id = slugger(text, seen)
    allHeadings.push({ id, text, level })
  }

  if (allHeadings.length === 0) return []

  // 2. Determine the topmost level present
  const minLevel = Math.min(...allHeadings.map(h => h.level))

  // 3. Filter: keep only two levels (top level and its immediate next)
  return allHeadings.filter(h => h.level === minLevel || h.level === minLevel + 1)
}
