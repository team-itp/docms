import React from 'react';
import { Route, Redirect } from 'react-router-dom';
import AppContext from '../AppContext';

class ProtectedRoute extends React.Component {
    render() {
      let { component: Component, ...rest } = this.props;
      let auth = this.context.getState().auth;
      return (
        <Route
          {...rest}
          render={props =>
            auth.isAuthenticated
            ? (<Component {...props} />)
            : (<Redirect to={{ pathname: "/login", state: { from: props.location } }} />)
          }
        />
      );
    }
  }
  
  ProtectedRoute.contextType = AppContext;

  export default ProtectedRoute;