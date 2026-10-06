import { useCallback, useEffect, useRef, useState } from 'react'
import { clampTime } from '../replay/replayMath'

type TimeListener = (t: number) => void

/** Playback clock of a replay. The fractional time lives in a ref and is pushed to subscribers (the canvas) on every
 * animation frame; React state only changes when the whole second, play state or speed changes, so the scrubber and
 * kill feed re-render at most a few times per second instead of 60. */
export function useReplayPlayback(durationSeconds: number) {
  const timeRef = useRef(0)
  const listeners = useRef(new Set<TimeListener>())
  const [second, setSecond] = useState(0)
  const [playing, setPlaying] = useState(false)
  const [speed, setSpeed] = useState(1)

  const publish = useCallback((t: number) => {
    timeRef.current = t
    listeners.current.forEach((listener) => listener(t))
    const whole = Math.floor(t)
    setSecond((current) => (current === whole ? current : whole))
  }, [])

  const subscribe = useCallback((listener: TimeListener) => {
    listeners.current.add(listener)
    listener(timeRef.current)
    return () => {
      listeners.current.delete(listener)
    }
  }, [])

  const seek = useCallback((t: number) => publish(clampTime(t, durationSeconds)), [durationSeconds, publish])

  const togglePlay = useCallback(() => {
    if (!playing && timeRef.current >= durationSeconds) publish(0)
    setPlaying(!playing)
  }, [playing, durationSeconds, publish])

  useEffect(() => {
    if (!playing) return

    let frame = 0
    let last = performance.now()
    const tick = (now: number) => {
      const next = clampTime(timeRef.current + ((now - last) / 1000) * speed, durationSeconds)
      last = now
      publish(next)
      if (next >= durationSeconds) {
        setPlaying(false)
        return
      }
      frame = requestAnimationFrame(tick)
    }

    frame = requestAnimationFrame(tick)
    return () => cancelAnimationFrame(frame)
  }, [playing, speed, durationSeconds, publish])

  return { timeRef, second, playing, speed, setSpeed, togglePlay, seek, subscribe }
}
