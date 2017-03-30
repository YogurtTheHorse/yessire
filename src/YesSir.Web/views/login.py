from app import app, lm
from app import app
from flask import request, redirect, render_template, url_for, flash
from flask_login import login_user, logout_user, login_required
from forms import LoginForm
from entities.user import User

@app.route('/login', methods=['GET', 'POST'])
@lm.unauthorized_handler
def login():
    form = LoginForm()
    error = None
    if request.method == 'POST' and form.validate_on_submit():
        user = app.config['USERS_COLLECTION'].find_one({"_id": form.username.data})
        if user and User.validate_login(user['password'], form.password.data):
            user_obj = User(user['_id'])
            login_user(user_obj)
            flash("Logged in successfully", category='success')

            return redirect(request.args.get("next") or url_for("play_page"))
        error = "Wrong username or password"

    return render_template('login.html', title='login', error=error, form=form)

@app.route('/logout')
@login_required
def logout():
    logout_user()
    return redirect(url_for('login'))

@lm.user_loader
def load_user(username):
    u = app.config['USERS_COLLECTION'].find_one({"_id": username})
    if not u:
        return None
    return User(u['_id'])

