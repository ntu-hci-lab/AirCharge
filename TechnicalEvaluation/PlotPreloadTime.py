import pandas as pd
import plotly.express as px
import matplotlib.pyplot as plt
import os
import glob
import numpy as np
import random
from collections import OrderedDict
targetAngles = [15,30,60,75,90]
pathAngleAll = [os.path.realpath('./Angle' + str(angle) + '/Preload') for angle in targetAngles]
targetFile = "TotalPreloadTime"
allFilesAngleAll = [glob.glob(pathAngle + "/"  + targetFile + "*.csv") for pathAngle in pathAngleAll]

plt.rcParams["figure.figsize"] = [6, 6]
plt.rcParams["figure.autolayout"] = True
headers = ['Time']
fig, axis = plt.subplots()
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
    df_tmp = None
    print("Degree {Angle}".format(Angle = targetAngles[index]))
    for file in files:
        if df_tmp is None:
            df_tmp = pd.read_csv(file,names=headers)
        # else:
            # df_tmp = pd.concat([df_tmp, pd.read_csv(file)], ignore_index = True)
    df_tmp = 1000 * df_tmp
    sum = 0
    for n in df_tmp['Time']:
        sum += float(n)

    print('Average Preload Time', sum/len(df_tmp['Time']))
    axis.plot(df_tmp, label = str(targetAngles[index]) + ' degree', color = [np.random.random(), np.random.random(), np.random.random()])
    
fig.tight_layout()
plt.title('Total Rewind Time'.format(PreloadTime=targetFile, Angle=0))
# plt.title('{Magnitude}'.format(Magnitude=targetFile))
plt.xticks(np.arange(0, 20, 1))
plt.yticks(np.arange(0, 1500, 100))
plt.xlabel('Count')
plt.ylabel('Time (ms)')
handles, labels = plt.gca().get_legend_handles_labels()
by_label = OrderedDict(zip(labels, handles))
plt.legend(by_label.values(), by_label.keys(), loc='upper right')

plt.show()
