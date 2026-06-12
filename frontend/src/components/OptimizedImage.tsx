"use client";

import Image from "next/image";
import { useState } from "react";

interface OptimizedImageProps {
  src: string;
  alt: string;
  width?: number;
  height?: number;
  fill?: boolean;
  className?: string;
  style?: React.CSSProperties;
  priority?: boolean;
  sizes?: string;
  quality?: number;
  onLoad?: () => void;
  containerClassName?: string;
  containerStyle?: React.CSSProperties;
  fallback?: string;
}

export default function OptimizedImage({
  src,
  alt,
  width,
  height,
  fill = false,
  className = "",
  style,
  priority = false,
  sizes,
  quality = 85,
  onLoad,
  containerClassName = "",
  containerStyle,
  fallback,
}: OptimizedImageProps) {
  const [imgSrc, setImgSrc] = useState(src);
  const [hasError, setHasError] = useState(false);

  // 如果图片加载失败，使用 fallback
  if (hasError && fallback) {
    return (
      <div
        className={`relative bg-gray-100 dark:bg-gray-800 flex items-center justify-center ${containerClassName}`}
        style={containerStyle}
      >
        <Image
          src={fallback}
          alt={alt}
          width={width}
          height={height}
          fill={fill}
          className={className}
          style={style}
          priority={priority}
          sizes={sizes}
          quality={quality}
          loading={priority ? undefined : "lazy"}
        />
      </div>
    );
  }

  return (
    <div
      className={`relative bg-gray-100 dark:bg-gray-800 ${containerClassName}`}
      style={containerStyle}
    >
      <Image
        src={imgSrc}
        alt={alt}
        width={width}
        height={height}
        fill={fill}
        className={className}
        style={style}
        priority={priority}
        sizes={sizes || (fill ? "100vw" : undefined)}
        quality={quality}
        loading={priority ? undefined : "lazy"}
        onError={() => {
          setHasError(true);
          if (fallback) setImgSrc(fallback);
        }}
        onLoad={onLoad}
      />
    </div>
  );
}