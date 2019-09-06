export const LOGIN_REQUESTED = 'LOGIN_REQUESTED';
export const LOGIN_ACCEPTED = 'LOGIN_ACCEPTED';
export const LOGIN_DENIED = 'LOGIN_DENIED';
export const SIGNOUT = 'SIGNOUT';

export function requestLogin(userName) {
  return { type: LOGIN_REQUESTED, userName };
}

export function authenticated(accessToken) {
  return { type: LOGIN_ACCEPTED, accessToken };
}

export function loginDenied(cause) {
  return { type: LOGIN_DENIED, cause };
}

export function signout() {
  return { type: SIGNOUT };
}

export const initialState = {
  isLoginRequesting: false,
  isAuthenticated: false,
  lastAuthenticated: null,
  userName: null,
  accessToken: null,
  loginFailedReason: null
}

export function authReducer(state = initialState, action) {
  switch (action.type) {
    case LOGIN_REQUESTED:
      return Object.assign({}, state, {
        isLoginRequesting: true,
        loginFailedReason: null,
        userName: action.userName
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
        userName: null,
        accessToken: null,
        loginFailedReason: null
      });
  }
}

export function login(userName, password) {
  return function (dispatch) {
    dispatch(requestLogin(userName, password));
    return fetch(`http://localhost:3000/account/login.json`)
      .then(
        response => response.json(),
        error => console.log('An error occurred.', error)
      )
      .then(json =>
        dispatch(authenticated(json.access_token))
      )
  }
}