#-------------------------------------------------------
# Obtain coords
import math
import random
import numpy as np
from numpy import asarray
import cv2
from PIL import Image, ImageDraw
import sys
from skimage.feature import hog
from sklearn import svm
from sklearn.metrics import classification_report,accuracy_score
import pickle
import sys
import os
import pathlib

folder_path =  str(pathlib.Path().absolute()) + "\\models"


#Obtain the path of the text file using OS argv
data_path = sys.argv[1]
#data_path = os.path.join(folder_path, "0_5.txt")

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

# # Preprocessing

# final_img_path = 'padded_cropped_shrinked_img.png'
# from keras.preprocessing import image
# img = image.load_img(final_img_path, color_mode="grayscale")
# x = image.img_to_array(img)
# x = np.expand_dims(x, axis=0)

# x = x.reshape(1, 784)
# x = x.astype('float32')
# # normalizing the data to help with the training
# x /= 255
 
# -------------------------------------------------------
# Prediction


def preprocess(img_path): #input an image   

    # load the image
    image = Image.open(img_path)
    # convert image to numpy array
    img_data = asarray(image)

    our_test = []
    our_test.append(img_data)


    our_test_resized = [cv2.resize(our_test[0], (80, 80))]

    ppc = 16
    hog_images = []
    hog_features = []
    for image in our_test_resized:
        fd,hog_image = hog(image, orientations=8, pixels_per_cell=(ppc,ppc),cells_per_block=(4, 4),block_norm= 'L2',visualize=True)
        hog_images.append(hog_image)
        hog_features.append(fd)

    hog_features = np.array(hog_features)
    return hog_features


#Test1
#folder_path  = "C:\\Users\\khale\\Source\\Repos\\Kinect-Drawing\\Python_part\\GP_2"
filename = os.path.join(folder_path, 'finalized_model_v3_LinesAdded.pkl')
loaded_model = pickle.load(open(filename, 'rb'))


#print("888")
print(loaded_model.predict(preprocess(final_img_path)))
#print("999")
