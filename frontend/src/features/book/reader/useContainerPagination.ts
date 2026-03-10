import type React from "react";
import { useLayoutEffect, useRef, useState } from "react";

import type { BookSection } from "../../../utils/bookReader/BookContent";

interface UseContainerPaginationArgs {
  fontScale: number;
  section: BookSection;
  onTotalPagesChange: (total: number) => void;
}

interface ContainerPaginationState {
  containerRef: React.RefObject<HTMLDivElement | null>;
  contentRef: React.RefObject<HTMLDivElement | null>;
  containerWidth: number;
  containerWidthRef: React.RefObject<number>;
}

export const useContainerPagination = ({
  fontScale,
  section,
  onTotalPagesChange,
}: UseContainerPaginationArgs): ContainerPaginationState => {
  const containerRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(0);
  const containerWidthRef = useRef(0);

  useLayoutEffect(() => {
    const container = containerRef.current;
    const content = contentRef.current;
    if (!container || !content) {
      return;
    }

    const measure = () => {
      const { width } = content.getBoundingClientRect();
      setContainerWidth(width);
      containerWidthRef.current = width;
      const pages =
        width > 0 ? Math.ceil(content.scrollWidth / width - 0.01) : 1;
      onTotalPagesChange(Math.max(1, pages));
    };

    measure();

    const observer = new ResizeObserver(measure);
    observer.observe(container);
    return () => observer.disconnect();
  }, [fontScale, section, onTotalPagesChange]);

  return { containerRef, contentRef, containerWidth, containerWidthRef };
};
