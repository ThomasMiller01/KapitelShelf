import { useDrag } from "@use-gesture/react";
import type { TransitionEvent } from "react";
import { useLayoutEffect, useRef, useState } from "react";

const DISTANCE_THRESHOLD = 0.5; // More than 25% of container width
const VELOCITY_THRESHOLD = 0.5;
const VELOCITY_MIN_DISTANCE_RATIO = 0.08;

export interface BoundarySwipeTransition {
  direction: "forward" | "backward";
  releasedOffset: number;
}

interface UseSwipeNavigationArgs {
  containerWidth: number;
  effectivePage: number;
  isAtSectionStart: boolean;
  isAtSectionEnd: boolean;
  canGoBack: boolean;
  canGoForward: boolean;
  isSectionTransitioning: boolean;
  onNext: () => void;
  onPrev: () => void;
  onBoundarySwipeCommit: (transition: BoundarySwipeTransition) => void;
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
  isAtSectionStart,
  isAtSectionEnd,
  canGoBack,
  canGoForward,
  isSectionTransitioning,
  onNext,
  onPrev,
  onBoundarySwipeCommit,
}: UseSwipeNavigationArgs): SwipeNavigationState => {
  const [dragOffset, setDragOffset] = useState(0);
  const [isSwiping, setIsSwiping] = useState(false);
  const [isSnapping, setIsSnapping] = useState(false);
  const dragOffsetRef = useRef(0);

  // Track pending commit to sync with async URL page update.
  const pendingCommitRef = useRef<{
    direction: "next" | "prev";
    targetPage: number;
  } | null>(null);

  // Refs for latest callback values (avoid stale closures in gesture handler).
  const onNextRef = useRef(onNext);
  const onPrevRef = useRef(onPrev);
  const onBoundarySwipeCommitRef = useRef(onBoundarySwipeCommit);
  onNextRef.current = onNext;
  onPrevRef.current = onPrev;
  onBoundarySwipeCommitRef.current = onBoundarySwipeCommit;

  const canGoBackRef = useRef(canGoBack);
  const canGoForwardRef = useRef(canGoForward);
  canGoBackRef.current = canGoBack;
  canGoForwardRef.current = canGoForward;

  const isAtSectionStartRef = useRef(isAtSectionStart);
  const isAtSectionEndRef = useRef(isAtSectionEnd);
  isAtSectionStartRef.current = isAtSectionStart;
  isAtSectionEndRef.current = isAtSectionEnd;

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
      dragOffsetRef.current = 0;
      setDragOffset(0);
    }
  }, [effectivePage]);

  // Reset swipe state when a section transition starts.
  useLayoutEffect(() => {
    if (isSectionTransitioning) {
      dragOffsetRef.current = 0;
      setDragOffset(0);
      setIsSwiping(false);
      setIsSnapping(false);
      pendingCommitRef.current = null;
    }
  }, [isSectionTransitioning]);

  const bindSwipe = useDrag(
    ({
      movement: [mx],
      velocity: [velocityX],
      direction: [directionX],
      active,
      last,
      cancel,
      first,
    }) => {
      const width = containerWidthRef.current;
      if (width === 0) {
        return;
      }

      // Don't start new gestures during section transitions or pending commits.
      if (
        first &&
        (isSectionTransitioningRef.current ||
          pendingCommitRef.current !== null)
      ) {
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

        dragOffsetRef.current = clamped;
        setDragOffset(clamped);
        setIsSwiping(true);
        return;
      }

      if (last) {
        setIsSwiping(false);

        const releasedOffset = mx === 0 ? dragOffsetRef.current : mx;
        const absMx = Math.abs(releasedOffset);
        const passedDistanceThreshold = absMx > width * DISTANCE_THRESHOLD;
        const passedVelocityThreshold =
          absMx >= width * VELOCITY_MIN_DISTANCE_RATIO &&
          Math.abs(velocityX) >= VELOCITY_THRESHOLD &&
          directionX !== 0;

        const releaseDirection =
          passedDistanceThreshold
            ? Math.sign(releasedOffset)
            : passedVelocityThreshold
              ? directionX
              : 0;
        const isForward = releaseDirection < 0; // dragging left = forward
        const isBackward = releaseDirection > 0; // dragging right = backward

        const shouldCommitForward =
          isForward && canGoForwardRef.current;
        const shouldCommitBackward =
          isBackward && canGoBackRef.current;
        const boundaryReleasedOffset = Math.max(
          -width,
          Math.min(width, releasedOffset),
        );
        const shouldCommitNextSection =
          shouldCommitForward && isAtSectionEndRef.current;
        const shouldCommitPrevSection =
          shouldCommitBackward && isAtSectionStartRef.current;

        if (shouldCommitNextSection) {
          dragOffsetRef.current = boundaryReleasedOffset;
          setDragOffset(boundaryReleasedOffset);
          onBoundarySwipeCommitRef.current({
            direction: "forward",
            releasedOffset: boundaryReleasedOffset,
          });
          onNextRef.current();
        } else if (shouldCommitPrevSection) {
          dragOffsetRef.current = boundaryReleasedOffset;
          setDragOffset(boundaryReleasedOffset);
          onBoundarySwipeCommitRef.current({
            direction: "backward",
            releasedOffset: boundaryReleasedOffset,
          });
          onPrevRef.current();
        } else if (shouldCommitForward) {
          // Snap to next page position.
          dragOffsetRef.current = -width;
          setDragOffset(-width);
          setIsSnapping(true);
        } else if (shouldCommitBackward) {
          // Snap to previous page position.
          dragOffsetRef.current = width;
          setDragOffset(width);
          setIsSnapping(true);
        } else {
          // Snap back to current page.
          if (releasedOffset === 0) {
            // No movement, no animation needed.
            dragOffsetRef.current = 0;
            setDragOffset(0);
          } else {
            dragOffsetRef.current = 0;
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

    if (dragOffsetRef.current === -containerWidth) {
      // Forward commit: keep offset until page updates.
      pendingCommitRef.current = {
        direction: "next",
        targetPage: effectivePage + 1,
      };
      onNextRef.current();
    } else if (dragOffsetRef.current === containerWidth) {
      // Backward commit: keep offset until page updates.
      pendingCommitRef.current = {
        direction: "prev",
        targetPage: effectivePage - 1,
      };
      onPrevRef.current();
    } else {
      // Snap-back complete, just reset.
      dragOffsetRef.current = 0;
      setDragOffset(0);
    }
  };

  return { dragOffset, isSwiping, isSnapping, onTransitionEnd, bindSwipe };
};
