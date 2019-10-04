import { combineReducers } from 'redux';
import authReducer from './authReducer';
import documentsReducer from './documentsReducer';
import { RESTORE_STATE } from '../actions/persistence';

const appReducer = combineReducers({
  auth: authReducer,
  documents: documentsReducer
});

const appReducerDecorator = (state, action) => {
  if (action.type === RESTORE_STATE) {
    const savedState = JSON.parse(localStorage.getItem('docms-client-app-store'));
    if (savedState) {
      state = savedState;
    }
  }
  return appReducer(state, action);
}

export default appReducerDecorator;