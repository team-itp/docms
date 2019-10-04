import { createStore, applyMiddleware } from 'redux';
import reducers from './reducers';
import api from './middlewares/api';
import { restoreState, SAVE_STATE } from './actions/persistence';

/**
 * Logs all actions and states after they are dispatched.
 */
const logger = store => next => action => {
  console.group(action.type)
  console.info('dispatching', action)
  let result = next(action)
  console.log('next state', store.getState())
  console.groupEnd()
  return result
}

/**
 * Sends crash reports as state is updated and listeners are notified.
 */
const crashReporter = store => next => action => {
  try {
    return next(action)
  } catch (err) {
    console.error('Caught an exception!', err)
    throw err
  }
}

const persister = ({ getState }) => next => action => {
  const stateBefore = getState();
  next(action);
  const stateAfter = getState();
  if (stateBefore !== stateAfter) {
    localStorage.setItem('docms-client-app-store', JSON.stringify(stateAfter));
  }

  if (action.type === SAVE_STATE) {
    const state = getState();
    localStorage.setItem('docms-client-app-store', JSON.stringify(state));
  }
}

const store = createStore(reducers,
  applyMiddleware(
    logger,
    crashReporter,
    persister,
    ...api
  ));

store.dispatch(restoreState());

export default store;