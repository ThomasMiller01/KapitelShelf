import { useDrag } from "@use-gesture/react";
import type { TransitionEvent } from "react";
import { useLayoutEffect, useRef, useState } from "react";

const DISTANCE_THRESHOLD = 0.25; // 25% of container width
const VELOCITY_THRESHOLD = 0.5; // @use-gesture velocity units

interface UseSwipeNavigationArgs {
  containerWidth: number;
  effectivePage: number;
  canGoBack: boolean;
  canGoForward: boolean;
  isSectionTransitioning: boolean;
  onNext: () => void;
  onPrev: () => void;
}

interface SwipeNavigationState {
  dragOffset: number;
  isSwiping: boolean;
  isSnapping: boolean;
  onTransitionEnd: (event: TransitionEvent<HTMLDivElement>) => void;
  bindSwipe: (...args: unknown[]) => Record<string, unknown>;
}

export const useSwipeNavigation = ({
  containerWidth,
  effectivePage,
  canGoBack,
  canGoForward,
  isSectionTransitioning,
  onNext,
  onPrev,
}: UseSwipeNavigationArgs): SwipeNavigationState => {
  const [dragOffset, setDragOffset] = useState(0);
  const [isSwiping, setIsSwiping] = useState(false);
  const [isSnapping, setIsSnapping] = useState(false);

  // Track pending commit to sync with async URL page update.
  const pendingCommitRef = useRef<{
    direction: "next" | "prev";
    targetPage: number;
  } | null>(null);

  // Refs for latest callback values (avoid stale closures in gesture handler).
  const onNextRef = useRef(onNext);
  const onPrevRef = useRef(onPrev);
  onNextRef.current = onNext;
  onPrevRef.current = onPrev;

  const canGoBackRef = useRef(canGoBack);
  const canGoForwardRef = useRef(canGoForward);
  canGoBackRef.current = canGoBack;
  canGoForwardRef.current = canGoForward;

  const containerWidthRef = useRef(containerWidth);
  containerWidthRef.current = containerWidth;

  const isSectionTransitioningRef = useRef(isSectionTransitioning);
  isSectionTransitioningRef.current = isSectionTransitioning;

  // When effectivePage matches the pending commit target, the URL has caught up — reset offset.
  useLayoutEffect(() => {
    if (
      pendingCommitRef.current !== null &&
      effectivePage === pendingCommitRef.current.targetPage
    ) {
      pendingCommitRef.current = null;
      setDragOffset(0);
    }
  }, [effectivePage]);

  // Reset swipe state when a section transition starts.
  useLayoutEffect(() => {
    if (isSectionTransitioning) {
      setDragOffset(0);
      setIsSwiping(false);
      setIsSnapping(false);
      pendingCommitRef.current = null;
    }
  }, [isSectionTransitioning]);

  const bindSwipe = useDrag(
    ({ movement: [mx], velocity: [vx], direction: [dx], active, last, cancel, first }) => {
      const width = containerWidthRef.current;
      if (width === 0) {
        return;
      }

      // Don't start new gestures during section transitions or pending commits.
      if (first && (isSectionTransitioningRef.current || pendingCommitRef.current !== null)) {
        cancel();
        return;
      }

      if (active) {
        // Clamp at bounds: positive mx = dragging right = going back, negative = going forward.
        let clamped = mx;
        if (!canGoBackRef.current && mx > 0) {
          clamped = 0;
        }

        if (!canGoForwardRef.current && mx < 0) {
          clamped = 0;
        }

        setDragOffset(clamped);
        setIsSwiping(true);
        return;
      }

      if (last) {
        setIsSwiping(false);

        const absMx = Math.abs(mx);
        const isForward = dx < 0; // dragging left = forward
        const isBackward = dx > 0; // dragging right = backward

        const passedThreshold =
          absMx > width * DISTANCE_THRESHOLD || vx > VELOCITY_THRESHOLD;

        const shouldCommitForward = passedThreshold && isForward && canGoForwardRef.current;
        const shouldCommitBackward = passedThreshold && isBackward && canGoBackRef.current;

        if (shouldCommitForward) {
          // Snap to next page position.
          setDragOffset(-width);
          setIsSnapping(true);
        } else if (shouldCommitBackward) {
          // Snap to previous page position.
          setDragOffset(width);
          setIsSnapping(true);
        } else {
          // Snap back to current page.
          if (mx === 0) {
            // No movement, no animation needed.
            setDragOffset(0);
          } else {
            setDragOffset(0);
            setIsSnapping(true);
          }
        }
      }
    },
    {
      axis: "x",
      filterTaps: true,
      pointer: { touch: true },
    },
  );

  // Called when the CSS transition on the content element ends.
  const onTransitionEnd = (event: TransitionEvent<HTMLDivElement>): void => {
    if (event.currentTarget !== event.target || event.propertyName !== "transform") {
      return;
    }

    if (!isSnapping) {
      return;
    }

    setIsSnapping(false);

    if (dragOffset === -containerWidth) {
      // Forward commit: keep offset until page updates.
      pendingCommitRef.current = {
        direction: "next",
        targetPage: effectivePage + 1,
      };
      onNextRef.current();
    } else if (dragOffset === containerWidth) {
      // Backward commit: keep offset until page updates.
      pendingCommitRef.current = {
        direction: "prev",
        targetPage: effectivePage - 1,
      };
      onPrevRef.current();
    } else {
      // Snap-back complete, just reset.
      setDragOffset(0);
    }
  };

  return { dragOffset, isSwiping, isSnapping, onTransitionEnd, bindSwipe };
};
