
# -------------------------------------------------------
# Obtain coords
import math
import random
import numpy as np
from numpy import asarray
import cv2
import os
from PIL import Image, ImageDraw
import sys
from skimage.feature import hog
from sklearn import svm
from sklearn.metrics import classification_report,accuracy_score
import pickle
import sys









import os

def do_it_all(data_path):
    #print("111")
    #Preprocessing and obtain the data coord from the txt file
    f = open(data_path, "r")
    _ = f.readline()

    data = f.readline()

    f.close()

    data_list = data.split(" ")
    data_list_coordinates = []
    for item in data_list:
        (x, y) = item.split(",")
        (x, y) = math.ceil(float(x)), math.ceil(float(y))
        data_list_coordinates.append((x, y))
    # -------------------------------------------------------
    #print("222")
    # Construct binary image using coords
    my_screen_width = 1920
    my_screen_height = 1080
    # let's create a 6 x 6 matrix with all pixels in black color
    img = np.zeros((my_screen_width, my_screen_height), np.uint8)

    for data in data_list_coordinates:
        img[data[0], data[1]] = 255

    cv2.imwrite("t1.png", img)
    # -------------------------------------------------------
    #print("333")
    # Mirroring
    # load the image, create the mirrored image, and the result placeholder
    img = Image.open("t1.png")
    mirror = img.transpose(Image.FLIP_LEFT_RIGHT).transpose(Image.ROTATE_90)
    mirror.save("t1.png")
    # -------------------------------------------------------
    #print("444")
    # Connect points using a thick line
    # from google.colab.patches import cv2_imshow
    img = cv2.imread("t1.png")
    (pre_x, pre_y) = data_list_coordinates[0]
    for (x, y) in data_list_coordinates[1:]:
        img = cv2.line(img, (pre_x, pre_y), (x, y), (255, 255, 255), 40)
        (pre_x, pre_y) = (x, y)

    # save our image as a "png" image
    # cv2_imshow(img)
    cv2.imwrite("t2.png", img)

    # -------------------------------------------------------
    #print("555")
    # Cropping
    img_orig = cv2.imread('t2.png', 0)

    mask = np.zeros(img_orig.shape, np.uint8)  # mask image the final image without small pieces

    # using findContours func to find the none-zero pieces
    contours, hierarchy = cv2.findContours(img_orig, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)

    # draw the white paper and eliminate the small pieces (less than 1000000 px). This px count is the same as the QR code dectection
    index = 0
    for cnt in contours:
        if cv2.contourArea(cnt) > 100:
            cv2.drawContours(mask, [cnt], 0, 255,
                             -1)  # the [] around cnt and 3rd argument 0 mean only the particular contour is drawn

            # Build a ROI to crop the QR
            x, y, w, h = cv2.boundingRect(cnt)
            roi = mask[y:y + h, x:x + w]
            # crop the original QR based on the ROI
            img_crop = img_orig[y:y + h, x:x + w]
            # use cropped mask image (roi) to get rid of all small pieces
            img_final = img_crop * (roi / 255)

    cv2.imwrite('t2_cropped.png', img_final)
    # -------------------------------------------------------
    #print("666")
    # Padding

    # read image
    img = cv2.imread('t2_cropped.png')
    ht, wd, cc = img.shape
    ww = hh = (math.ceil(max(wd, ht) / 28) + 1) * 28

    # create new image of desired size and color (blue) for padding
    color = (0, 0, 0)
    result = np.full((hh, ww, cc), color, dtype=np.uint8)

    # compute center offset
    xx = (ww - wd) // 2
    yy = (hh - ht) // 2

    # copy img image into center of result image
    result[yy:yy + ht, xx:xx + wd] = img

    # view result
    # cv2_imshow(result)
    # save result
    cv2.imwrite("padded_cropped_img.png", result)
    # -------------------------------------------------------
    #print("777")
    # resizing PIL   ->perfect
    image = Image.open('padded_cropped_img.png')
    new_image = image.resize((28, 28))
    new_image.save('padded_cropped_shrinked_img.png')
    final_img_path = 'padded_cropped_shrinked_img.png'
    # -------------------------------------------------------
    # Collecting data for training


    # load the image
    image = Image.open(final_img_path)
    # convert image to numpy array
    img_data = asarray(image)
    return img_data
def hog_alorithm(training_data):
    training_data_resized = []
    for img in training_data:
        training_data_resized.append(cv2.resize(img, (80, 80)))

    ppc = 16
    hog_images = []
    hog_features = []
    for image in training_data_resized:
        fd,hog_image = hog(image, orientations=8, pixels_per_cell=(ppc,ppc),cells_per_block=(4, 4),block_norm= 'L2',visualize=True)
        hog_images.append(hog_image)
        hog_features.append(fd)

    print(len(hog_features))
    hog_features = np.array(hog_features)
    return hog_features

training_data = []
classes_sizes = []
labels = []
data_path = "C:\\Users\\khale\\OneDrive\\Desktop\\Amr Data"

for class_path in os.listdir(data_path):

    class_path = os.path.join(data_path, class_path)

    class_len = len(os.listdir(class_path))
    classes_sizes.append(class_len)

    for shape_path in os.listdir(class_path):
        print(shape_path)
        shape_path = os.path.join(class_path, shape_path)

        if os.stat(shape_path).st_size == 0:
            os.remove(shape_path)
            continue

        training_data.append(do_it_all(shape_path))

hog_features = hog_alorithm(training_data)


labels = np.zeros((classes_sizes[0], 1), dtype="int8")
for i in range(1, len(classes_sizes)):
    label_i = np.zeros((classes_sizes[i], 1), dtype="int8") + i
    labels = np.concatenate((labels, label_i))

clf = svm.SVC()
data_frame = np.hstack((hog_features, labels))
np.random.shuffle(data_frame)

percentage = 80
partition = int(len(hog_features)*percentage/100)

x_train, x_test = data_frame[:partition,:-1],  data_frame[partition:,:-1]
y_train, y_test = data_frame[:partition,-1:].ravel() , data_frame[partition:,-1:].ravel()

clf.fit(x_train, y_train)

import pickle
# save the model to disk
filename = 'finalized_model.pkl'
pickle.dump(clf, open(filename, 'wb'))