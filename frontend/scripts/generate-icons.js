/**
 * 生成 PWA 图标（纯 Node.js，无外部依赖）
 * 生成 192x192 和 512x512 两个 indigo 底色 PNG 图标
 */
const fs = require('fs')
const path = require('path')
const zlib = require('zlib')

const SIZES = [192, 512]
const COLOR_BG = [99, 102, 241, 255]    // #6366f1 (indigo)
const COLOR_CIRCLE = [129, 140, 248, 255] // #818cf8 (lighter indigo)
const OUT_DIR = path.join(__dirname, '..', 'public', 'icons')

/** 创建 PNG 文件二进制 */
function createPng(width, height, pixels) {
  // 1. PNG signature
  const signature = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10])
  
  // 2. IHDR chunk
  const ihdr = Buffer.alloc(13)
  ihdr.writeUInt32BE(width, 0)
  ihdr.writeUInt32BE(height, 4)
  ihdr[8] = 8  // bit depth
  ihdr[9] = 6  // color type (RGBA)
  ihdr[10] = 0 // compression
  ihdr[11] = 0 // filter
  ihdr[12] = 0 // interlace
  const ihdrChunk = createChunk('IHDR', ihdr)
  
  // 3. IDAT chunk (raw pixel data)
  const rawData = Buffer.alloc(height * (1 + width * 4))
  for (let y = 0; y < height; y++) {
    rawData[y * (1 + width * 4)] = 0 // filter byte = None
    for (let x = 0; x < width; x++) {
      const idx = y * (1 + width * 4) + 1 + x * 4
      const pi = (y * width + x) * 4
      rawData[idx] = pixels[pi]
      rawData[idx + 1] = pixels[pi + 1]
      rawData[idx + 2] = pixels[pi + 2]
      rawData[idx + 3] = pixels[pi + 3]
    }
  }
  const compressed = zlib.deflateSync(rawData)
  const idatChunk = createChunk('IDAT', compressed)
  
  // 4. IEND chunk
  const iendChunk = createChunk('IEND', Buffer.alloc(0))
  
  return Buffer.concat([signature, ihdrChunk, idatChunk, iendChunk])
}

/** 创建 PNG chunk */
function createChunk(type, data) {
  const length = Buffer.alloc(4)
  length.writeUInt32BE(data.length, 0)
  const typeBuffer = Buffer.from(type, 'ascii')
  const crcData = Buffer.concat([typeBuffer, data])
  const crc = crc32(crcData)
  const crcBuffer = Buffer.alloc(4)
  crcBuffer.writeUInt32BE(crc, 0)
  return Buffer.concat([length, typeBuffer, data, crcBuffer])
}

/** CRC32 计算 */
function crc32(data) {
  let crc = 0xFFFFFFFF
  for (let i = 0; i < data.length; i++) {
    crc ^= data[i]
    for (let j = 0; j < 8; j++) {
      crc = (crc >>> 1) ^ (crc & 1 ? 0xEDB88320 : 0)
    }
  }
  return (crc ^ 0xFFFFFFFF) >>> 0
}

/** 生成圆角矩形图标像素 */
function generateIconPixels(size) {
  const pixels = new Uint8Array(size * size * 4)
  const radius = size * 0.22
  const centerCircleR = size * 0.3
  const cx = size / 2
  const cy = size / 2
  
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const idx = (y * size + x) * 4
      
      // 圆角矩形裁剪（四个角）
      const dx = x < radius ? radius - x : (x > size - radius ? x - (size - radius) : 0)
      const dy = y < radius ? radius - y : (y > size - radius ? y - (size - radius) : 0)
      const dist = Math.sqrt(dx * dx + dy * dy)
      
      if (dist > radius) {
        // 超出圆角范围 — 透明
        pixels[idx] = 0
        pixels[idx + 1] = 0
        pixels[idx + 2] = 0
        pixels[idx + 3] = 0
        continue
      }
      
      // 背景色
      pixels[idx] = COLOR_BG[0]
      pixels[idx + 1] = COLOR_BG[1]
      pixels[idx + 2] = COLOR_BG[2]
      pixels[idx + 3] = COLOR_BG[3]
      
      // 中心圆形（较浅色）
      const distFromCenter = Math.sqrt((x - cx) ** 2 + (y - cy) ** 2)
      if (distFromCenter < centerCircleR) {
        pixels[idx] = COLOR_CIRCLE[0]
        pixels[idx + 1] = COLOR_CIRCLE[1]
        pixels[idx + 2] = COLOR_CIRCLE[2]
        pixels[idx + 3] = COLOR_CIRCLE[3]
      }
    }
  }
  return pixels
}

/** 主函数 */
function main() {
  // 创建输出目录
  if (!fs.existsSync(OUT_DIR)) {
    fs.mkdirSync(OUT_DIR, { recursive: true })
  }

  for (const size of SIZES) {
    const pixels = generateIconPixels(size)
    const png = createPng(size, size, pixels)
    const filePath = path.join(OUT_DIR, `icon-${size}.png`)
    fs.writeFileSync(filePath, png)
    const fileSize = (fs.statSync(filePath).size / 1024).toFixed(1)
    console.log(`  ✓ icon-${size}.png (${fileSize} KB)`)
  }
  console.log('  PWA 图标生成完成')
}

main()