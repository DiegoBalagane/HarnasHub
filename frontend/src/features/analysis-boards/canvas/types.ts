/** One point of a freehand stroke, as a fraction [0,1] of the canvas — resolution-independent, same convention as
 * every other pin/position on this app's maps (MapPositionAssignment.X/Y, etc.). */
export interface StrokePoint {
  x: number
  y: number
}

/** One continuous pen stroke. `width` is in a fixed REFERENCE_SIZE-pixel space, scaled to the canvas's actual
 * rendered size at draw time, so a stroke looks the same thickness regardless of viewport width. */
export interface Stroke {
  color: string
  width: number
  points: StrokePoint[]
}

/** The logical canvas width a stroke's `width` is expressed against. */
export const REFERENCE_SIZE = 1000
