import { SAVE_STATE } from "../actions/persistence";

const persistence = ({ getState }) => next => action => {
  next(action);

  if (action.type === SAVE_STATE) {
    const state = getState();
    console.log(state);
    localStorage.setItem('docms-client-app-store', JSON.stringify(state));
  }
}

export default [persistence];