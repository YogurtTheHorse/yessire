from app import app, socketio, db_web
from flask import render_template, redirect, url_for

from flask_socketio import emit

from flask_login import current_user, login_required
import functools

from requests import get
from urllib.parse import quote
import json

MESSAGE_HISTORY_SIZE = 100


def authenticated_only(f):
    @functools.wraps(f)
    def wrapped(*args, **kwargs):
        if not current_user.is_authenticated:
            disconnect()
        else:
            return f(*args, **kwargs)
    return wrapped


@app.route('/play')
def play_page():
    if current_user.is_authenticated:
        return render_template('play.html')
    else:
        return redirect(url_for('login'))

@socketio.on('connect')
def on_connect():
    print('User connected')

@socketio.on('startGame')
@authenticated_only
def handle_start():
    user_id = current_user.name
    chat_history_clear(current_user.username)
    return emit('startGame', method('/start/web/%s' % current_user.username))

@socketio.on('sendMessage')
@authenticated_only
def handle_send_message(message):
    chat_history_add({"Message": {"From": current_user.username, "Text": message['msg']}}, current_user.username)
    return emit('sendMessage', method('/message/web/%s/%s' % (current_user.username, message['msg'])))

@socketio.on('getMessages')
@authenticated_only
def handle_get_messages():
    method_result = method('/get/web/%s' % current_user.username)
    for message in json.loads(method_result):
        print(message)
        chat_history_add(message, current_user.username)

    return emit('getMessages', method_result)

@socketio.on('getMessageHistory')
@authenticated_only
def handle_get_chat_history():
    return emit('getMessages', chat_history_get(current_user.username))

@socketio.on('message')
@authenticated_only
def test(message):
    print(message)
    return

def method(path):
    '''HTTP-request'''
    request = get('http://localhost:9797' + quote(path))
    return request.text

def chat_history_add(text, user_id):
    user_data = db_web.find_one({'user_id': user_id})
    if not user_data:
        # create new user doc
        user_data = db_web.insert_one({'user_id': user_id, 'messages': [text]})

    else:
        # update user doc
        new_user_data = db_web.update_one(user_data,
                        {'$push': {'messages': {'$each': [text], '$slice': -MESSAGE_HISTORY_SIZE}}})

def chat_history_get(user_id):
    user_data = db_web.find_one({'user_id': user_id})
    if user_data:
        messages = user_data['messages']
        return json.dumps(messages)

def chat_history_clear(user_id):
    return db_web.delete_one({'user_id': user_id})
