from app import app
from flask import render_template
HTTP_OK_CODE = 200

@app.route('/')
@app.route('/index')
def index():
    '''Home page'''
    return render_template('index.html')

@app.route('/subscribe/<email>', methods=['POST'])
def subscribe(email):
    '''POST-request for subscribe email'''

    with open('subscriptions.txt', 'a') as f:
        f.write(email + '\n')
        print('Subscribed: ' + email)

    return '', HTTP_OK_CODE