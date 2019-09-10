import React from 'react';
import { withRouter } from 'react-router-dom';
import { connect } from 'react-redux';
import { AppBar, Button, Toolbar, Typography } from '@material-ui/core';
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



function AppTitleBar() {
  const classes = useStyles();
  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" className={classes.title}>
          文書管理システム
        </Typography>
        <AuthButton />
      </Toolbar>
    </AppBar>
  );
}

export default AppTitleBar;
