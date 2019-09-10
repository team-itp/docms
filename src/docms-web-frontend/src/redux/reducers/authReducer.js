import {
  LOGIN_REQUESTED,
  LOGIN_ACCEPTED,
  LOGIN_DENIED,
  SIGNOUT
} from "../actions";

const initialState = {
  isLoginRequesting: false,
  isAuthenticated: false,
  lastAuthenticated: null,
  accessToken: null,
  loginFailedReason: null
}

function authReducer(state = initialState, action) {
  switch (action.type) {
    case LOGIN_REQUESTED:
      return Object.assign({}, state, {
        isLoginRequesting: true,
        loginFailedReason: null
      });
    case LOGIN_ACCEPTED:
      return Object.assign({}, state, {
        isLoginRequesting: false,
        isAuthenticated: true,
        lastAuthenticated: new Date(),
        accessToken: action.accessToken
      });
    case LOGIN_DENIED:
      return Object.assign({}, state, {
        isLoginRequesting: false,
        loginFailedReason: action.cause
      });
    case SIGNOUT:
      return Object.assign({}, state, {
        isLoginRequesting: false,
        isAuthenticated: false,
        accessToken: null,
        loginFailedReason: null
      });
    default:
      return state;
  }
}

export default authReducer;
