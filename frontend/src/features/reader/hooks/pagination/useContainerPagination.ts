import { type RefObject, useLayoutEffect, useRef, useState } from "react";

import type { BookSection } from "../../utils/BookContentModels";

interface Measurement {
  containerRef: RefObject<HTMLDivElement | null>;
  contentRef: RefObject<HTMLDivElement | null>;
  pageStride: number;
}

export const useContainerPagination = (
  section: BookSection,
  fontScale: number,
  fontFamily: string,
  onMeasure: (pages: number, resized: boolean) => void,
  gap: number,
): Measurement => {
  const containerRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [pageStride, setPageStride] = useState(0);
  const previous = useRef<{ section: BookSection; width: number; height: number; fontScale: number; fontFamily: string }>(undefined);

  useLayoutEffect(() => {
    const container = containerRef.current;
    const content = contentRef.current;
    if (!container || !content) {
      return;
    }

    let active = true;
    const measure = (): void => {
      const width = content.offsetWidth;
      const height = content.offsetHeight;
      if (!active || width === 0 || height === 0) {
        return;
      }
      const prior = previous.current;
      const resized = prior?.section === section && (prior.width !== width || prior.height !== height
        || prior.fontScale !== fontScale || prior.fontFamily !== fontFamily);
      previous.current = { section, width, height, fontScale, fontFamily };
      const stride = width + gap;
      setPageStride(stride);
      onMeasure(Math.max(1, Math.ceil((content.scrollWidth + gap) / stride - 0.01)), resized);
    };

    const shadow = content.querySelector("[data-reader-content]")?.shadowRoot;
    shadow?.addEventListener("load", measure, true);
    void document.fonts.ready.then(measure);
    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(container);
    return (): void => {
      active = false;
      observer.disconnect();
      shadow?.removeEventListener("load", measure, true);
    };
  }, [fontFamily, fontScale, gap, onMeasure, section]);

  return { containerRef, contentRef, pageStride };
};
