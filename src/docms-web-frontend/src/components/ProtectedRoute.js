import React from 'react';
import { Route, Redirect, withRouter } from 'react-router-dom';
import { connect } from 'react-redux';

function ProtectedRoute(props) {
  let { component: Component, isAuthenticated, ...rest } = props;
  return (
  <Route
    {...rest} 
    render={props =>
      isAuthenticated
        ? (<Component {...props} />)
        : (<Redirect to={{ pathname: "/login", state: { from: props.location } }} />)
      }
  />
  );
}

function mapStateToProps(state, ownProps) {
  return {
    isAuthenticated: state.auth.isAuthenticated
  };
}

export default withRouter(connect(mapStateToProps)(ProtectedRoute));