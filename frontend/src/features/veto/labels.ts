import type { VetoAction, VetoActor, VetoRecommendation } from '../../services/vetoApi'

export const vetoActors: VetoActor[] = ['Us', 'Opponent']
export const vetoActions: VetoAction[] = ['Ban', 'Pick', 'Decider']

export const vetoActorLabels: Record<VetoActor, string> = {
  Us: 'My',
  Opponent: 'Przeciwnik',
}

export const vetoActionLabels: Record<VetoAction, string> = {
  Ban: 'ban',
  Pick: 'pick',
  Decider: 'decider',
}

export const vetoRecommendationLabels: Record<VetoRecommendation, string> = {
  Pick: 'Pick',
  Ban: 'Ban',
  Neutral: '—',
}

/** Tailwind classes for recommendation badges and recorded veto chips. */
export const vetoActionClasses: Record<VetoAction | 'Neutral', string> = {
  Pick: 'border-success-600 bg-success-950/50 text-success-300',
  Ban: 'border-danger-700 bg-danger-950/50 text-danger-300',
  Decider: 'border-info-600 bg-info-950/50 text-info-300',
  Neutral: 'border-neutral-700 text-neutral-400',
}
