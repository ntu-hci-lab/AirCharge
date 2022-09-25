import pandas as pd
import plotly.express as px
import matplotlib.pyplot as plt
import os
import glob
import numpy as np
import random

# targetAngle = 90 
targetAngles = [45]
# path = os.path.realpath('./Angle' + str(targetAngle))
pathAngle0 = os.path.realpath('./Angle0')
# /Test/TimeForcePressurePlots_AirCharge/Datas
# pathAngleAll = [os.path.realpath('./Angle' + str(angle) + '/0901(L Rotary)') for angle in targetAngles]
pathAngleAll = [os.path.realpath('./20220703/Angle90') for angle in targetAngles]
"""
12	0.10 MPa    16	0.12 MPa    20	0.14 MPa
24	0.16 MPa    28	0.18 MPa    32	0.20 MPa
36	0.22 MPa    40	0.24 MPa    44	0.26 MPa
48	0.28 MPa    53	0.30 MPa    57	0.32 MPa
61	0.34 MPa    66	0.36 MPa    70	0.38 MPa
75	0.40 MPa    80	0.42 MPa    84	0.44 MPa
89	0.46 MPa    94	0.48 MPa    99	0.50 MPa
104	0.52 MPa    109	0.54 MPa    114	0.56 MPa
119	0.58 MPa    124	0.60 MPa    130	0.62 MPa
135	0.64 MPa    141	0.66 MPa    146	0.68 MPa
152	0.70 MPa	
"""
"""
400		1.0 bar	0.10 MPa    470		1.2 bar	0.12 MPa    550		1.4 bar	0.14 MPa
650		1.6 bar	0.16 MPa    730		1.8 bar	0.18 MPa    830		2.0 bar	0.20 MPa
910		2.2 bar	0.22 MPa    1020	2.4 bar	0.24 MPa    1120	2.6 bar	0.26 MPa
1220	2.8 bar	0.28 MPa    1300	3.0 bar	0.30 MPa    1400	3.2 bar	0.32 MPa
1450	3.4 bar	0.34 MPa    1550	3.6 bar	0.36 MPa    1650	3.8 bar	0.38 MPa
1750	4.0 bar	0.40 MPa    1850	4.2 bar	0.42 MPa    1950	4.4 bar	0.44 MPa
2050	4.6 bar	0.46 MPa    2100	4.8 bar	0.48 MPa    2200	5.0 bar	0.50 MPa
2300	5.2 bar	0.52 MPa    2400	5.4 bar	0.54 MPa    2500	5.6 bar	0.56 MPa
2600	5.8 bar	0.58 MPa    2700	6.0 bar	0.60 MPa    2800	6.2 bar	0.62 MPa
2900	6.4 bar	0.64 MPa    3000	6.6 bar	0.66 MPa    3100	6.8 bar	0.68 MPa
3200	7.0 bar	0.70 MPa				
"""
targetFile = "Force"
# allFiles = glob.glob(path + "/"  + targetFile + "*.csv")
allFilesAngle0 = glob.glob(pathAngle0 + "/"  + targetFile + "*.csv")
allFilesAngleAll = [glob.glob(pathAngle + "/"  + targetFile + "*.csv") for pathAngle in pathAngleAll]
# print(allFiles)
plt.rcParams["figure.figsize"] = [6, 6]
plt.rcParams["figure.autolayout"] = True
headers = ['Time', 'Force']

fig = plt.figure()
# fig, axis = plt.subplots()


# for index, file in enumerate(allFiles):
#     df_tmp = pd.read_csv(file,names=headers)
#     # 截掉Impact前的時間(preload time)
#     # for i in range(len(df_tmp)):
#         # if df_tmp['Force'][i] != 0:
#         #     df = pd.DataFrame({"StartTime":[df_tmp['Time'][i] for _ in range(len(df_tmp))]})  
#         #     df_tmp['Time'] = df_tmp['Time'] - df['StartTime']
#         #     break
#     df_tmp = df_tmp[0:2000]
#     axis.plot(df_tmp['Time'], df_tmp['Force'], color=[np.random.random(),np.random.random(),np.random.random()])

# for index, file in enumerate(allFilesAngle0):
#     df_tmp = pd.read_csv(file,names=headers)
#     df_tmp = df_tmp[0:2000]
#     axis.plot(df_tmp['Time'], df_tmp['Force'], color=[np.random.random(),np.random.random(),np.random.random()])

for index, files in enumerate(allFilesAngleAll):
    for count,file in enumerate(files):
        df_tmp = pd.read_csv(file,names=headers)
        df_tmp['Time'] = 1000 * df_tmp['Time']
        df_tmp = df_tmp[0:1000]
        # plt.subplot(1,1,count+1)
        # if count <= 4:
        #     plt.title('{JetTime} (Degree {Angle})'.format(JetTime='80ms', Angle=targetAngles[0]))
        # elif count <= 9:
        #     plt.title('{JetTime} (Degree {Angle})'.format(JetTime='90ms', Angle=targetAngles[0]))
        # elif count <= 14:
        #     plt.title('{JetTime} (Degree {Angle})'.format(JetTime='100ms', Angle=targetAngles[0]))
        # elif count <= 19:
        #     plt.title('{JetTime} (Degree {Angle})'.format(JetTime='110ms', Angle=targetAngles[0]))
        # elif count <= 24:
        #     plt.title('{JetTime} (Degree {Angle})'.format(JetTime='120ms', Angle=targetAngles[0]))
        # elif count <= 29:
        #     plt.title('{JetTime} (Degree {Angle})'.format(JetTime='200ms', Angle=targetAngles[0]))
        # plt.title(str(file)[140:150])
        plt.title('45 degrees force diagram')
        plt.plot(df_tmp['Time'], df_tmp['Force'], label = str(targetAngles[index]) + ' degree', color = [np.random.random(), np.random.random(), np.random.random()])
        # plt.xticks(np.arange(0, 1100, 100))
        # plt.yticks(np.arange(0, 40, 10))
        # axis.plot(df_tmp['Time'], df_tmp['Force'], label = str(targetAngles[index]) + ' degree', color = [np.random.random(), np.random.random(), np.random.random()])

    
fig.tight_layout()
# plt.title('{Magnitude} (Degree {Angle})'.format(Magnitude=targetFile, Angle=targetAngles[0]))
# plt.title('0.6MPa, 15 cm with rotary')
# plt.title('{Magnitude}'.format(Magnitude=targetFile))
plt.xlabel('Time (ms)')
plt.ylabel('Force (N)')
# plt.legend(loc='upper right')

# plt.xticks(np.arange(0, 1000, 100))
# plt.xticks(np.arange(0, 300, 10))
plt.show()
