import React from 'react';
import { withRouter } from 'react-router-dom';
import { connect } from 'react-redux';
import { AppBar, Button, Toolbar, Typography } from '@material-ui/core';
import SettingsIcon from '@material-ui/icons/Settings';
import { makeStyles } from '@material-ui/core/styles';
import { signout } from '../redux/actions';

const useStyles = makeStyles(theme => ({
  title: {
    flexGrow: 1,
  },
}));

function mapStateToProps(state, ownProps) {
  return {
    isAuthenticated: state.auth.isAuthenticated
  };
}

function mapDispatchToProps(dispatch, ownProps) {
  return {
    signout: () => dispatch(signout())
  };
}

const AppTitle = withRouter(
  ({ history }) => {
    const classes = useStyles();
    return (<Typography className={classes.title} variant="h6" onClick={() => history.push('/')}>
      文書管理システム
    </Typography>);
  }
);

const AuthButton = withRouter(
  connect(mapStateToProps, mapDispatchToProps)(
    ({ history, isAuthenticated, signout }) => {
      return (
        isAuthenticated
          ? (<Button color="inherit" onClick={() => {
            signout();
            history.push('/');
          }}>LOGOUT</Button>)
          : null
      );
    }));

const SettingsButton = withRouter(
  connect(mapStateToProps, mapDispatchToProps)(
    ({ history, isAuthenticated, isAdministrator }) => {
      return (
        (isAuthenticated && isAdministrator)
          ? (<Button color="inherit" onClick={() => {
            history.push('/admin');
          }}><SettingsIcon /></Button>)
          : null
      );
    }));



function AppTitleBar() {
  return (
    <AppBar position="static">
      <Toolbar>
        <AppTitle />
        <AuthButton />
        <SettingsButton />
      </Toolbar>
    </AppBar>
  );
}

export default AppTitleBar;
