import { combineReducers } from 'redux';
import authReducer from './authReducer';
import documentsReducer from './documentsReducer';

const appReducer = combineReducers({
    auth: authReducer,
    documents: documentsReducer
});

export default appReducer;