import React from 'react';
import { Redirect } from 'react-router-dom';
import { connect } from 'react-redux';
import { Button, Container, FormControlLabel, Grid, Switch, Typography, TextField } from '@material-ui/core';
import { requestLogin } from '../redux/actions';

class Login extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      userName: '',
      password: ''
    }
  }

  login() {
    this.props.login(this.state.userName, this.state.password);
  }

  handleUserNameChange(e) {
    this.setState({ ...this.state, userName: e.target.value });
  }

  handlePasswordChange(e) {
    this.setState({ ...this.state, password: e.target.value });
  }

  render() {
    const { from } = this.props.location.state || { from: { pathname: "/" } };
    const redirectToReferrer = this.props.auth.isAuthenticated;

    if (redirectToReferrer) return <Redirect to={from} />;

    return (
      <Container component="main" maxWidth="xl" style={{ marginTop: '20px' }}>
        <Grid container alignItems="center">
          <Typography component="h2" variant="h5">
            ログイン
          </Typography>
        </Grid>
        <form>
          <Grid container>
            <Grid item sm={6}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextField
                    required
                    fullWidth
                    label="ユーザー名"
                    value={this.state.userName}
                    onChange={this.handleUserNameChange.bind(this)} />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    required
                    fullWidth
                    type="password"
                    label="パスワード"
                    value={this.state.password}
                    onChange={this.handlePasswordChange.bind(this)} />
                </Grid>
                <Grid item xs={12}>
                  <FormControlLabel control={
                    <Switch />
                  } label="サインイン状態を保存する" />
                </Grid>
                <Grid item xs={12}>
                  <Button onClick={this.login.bind(this)} color="primary" variant="outlined">ログイン</Button>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
        </form>
      </Container>
    );
  }
}

function mapStateToProps(state, ownProps) {
  return {
    auth: state.auth
  };
}

function mapDispatchToProps(dispatch, ownProps) {
  return {
    login: (userName, password) => dispatch(requestLogin(userName, password))
  }
}

export default connect(mapStateToProps, mapDispatchToProps)(Login);