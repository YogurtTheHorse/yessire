from app import app
from flask import render_template
from flask_api import status

@app.route('/')
def index():
    '''Home page'''
    return render_template('index.html')

@app.route('/subscribe/<email>', methods=['POST'])
def subscribe(email):
    '''POST-request for subscribe email'''

    with open('subscriptions.txt', 'a') as f:
        f.write(email + '\n')
        print('Subscribed: ' + email)

    return '', status.HTTP_200_OK