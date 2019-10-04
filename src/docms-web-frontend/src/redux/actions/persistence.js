export const RESTORE_STATE = 'RESTORE_STATE';
export const SAVE_STATE = 'SAVE_STATE';

export function restoreState() {
  return { type: RESTORE_STATE };
}

export function saveState() {
  return { type: SAVE_STATE };
}