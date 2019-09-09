export const LOGIN_REQUESTED = 'LOGIN_REQUESTED';
export const LOGIN_ACCEPTED = 'LOGIN_ACCEPTED';
export const LOGIN_DENIED = 'LOGIN_DENIED';
export const SIGNOUT = 'SIGNOUT';

export function requestLogin({ userName, password }) {
  return { type: LOGIN_REQUESTED, payload: { userName, password } };
}

export function authenticated(accessToken) {
  return { type: LOGIN_ACCEPTED, payload: accessToken };
}

export function loginDenied(cause) {
  return { type: LOGIN_DENIED, payload: cause };
}

export function signout() {
  return { type: SIGNOUT };
}

export function login(userName, password) {
  return function (dispatch) {
    dispatch(requestLogin({ userName, password }));
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