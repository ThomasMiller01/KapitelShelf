import type React from "react";
import { useLayoutEffect, useRef, useState } from "react";

import type { BookSection } from "../../utils/BookContentModels";

interface UseContainerPaginationArgs {
  fontScale: number;
  section: BookSection;
  onTotalPagesChange: (total: number) => void;
  pageGap: number;
}

interface ContainerPaginationState {
  containerRef: React.RefObject<HTMLDivElement | null>;
  contentRef: React.RefObject<HTMLDivElement | null>;
  pageWidth: number;
  pageWidthRef: React.RefObject<number>;
  pageStride: number;
}

export const useContainerPagination = ({
  fontScale,
  section,
  onTotalPagesChange,
  pageGap,
}: UseContainerPaginationArgs): ContainerPaginationState => {
  const containerRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [pageWidth, setPageWidth] = useState(0);
  const pageWidthRef = useRef(0);

  useLayoutEffect(() => {
    const container = containerRef.current;
    const content = contentRef.current;
    if (!container || !content) {
      return;
    }

    const measure = () => {
      const { width } = content.getBoundingClientRect();
      const stride = width + pageGap;
      setPageWidth(width);
      pageWidthRef.current = width;
      const pages =
        width > 0 && stride > 0
          ? Math.max(
              1,
              Math.ceil((content.scrollWidth + pageGap) / stride - 0.01),
            )
          : 1;
      onTotalPagesChange(pages);
    };

    measure();

    const observer = new ResizeObserver(measure);
    observer.observe(container);
    return () => observer.disconnect();
  }, [fontScale, pageGap, section, onTotalPagesChange]);

  return {
    containerRef,
    contentRef,
    pageWidth,
    pageWidthRef,
    pageStride: pageWidth > 0 ? pageWidth + pageGap : 0,
  };
};
