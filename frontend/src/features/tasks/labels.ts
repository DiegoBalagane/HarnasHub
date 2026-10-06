import type { TaskStatus } from '../../services/tasksApi'

export const taskStatusLabels: Record<TaskStatus, string> = {
  Todo: 'Do zrobienia',
  PendingReview: 'Czeka na weryfikację',
  Done: 'Zaliczone',
  NeedsRework: 'Do poprawy',
}

export const taskStatusColors: Record<TaskStatus, string> = {
  Todo: 'text-neutral-400',
  PendingReview: 'text-warning-400',
  Done: 'text-success-400',
  NeedsRework: 'text-danger-400',
}
