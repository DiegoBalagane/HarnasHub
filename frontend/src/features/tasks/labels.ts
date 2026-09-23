import type { TaskStatus } from '../../services/tasksApi'

export const taskStatusLabels: Record<TaskStatus, string> = {
  Todo: 'Do zrobienia',
  PendingReview: 'Czeka na weryfikację',
  Done: 'Zaliczone',
  NeedsRework: 'Do poprawy',
}

export const taskStatusColors: Record<TaskStatus, string> = {
  Todo: 'text-neutral-400',
  PendingReview: 'text-yellow-400',
  Done: 'text-green-400',
  NeedsRework: 'text-red-400',
}
